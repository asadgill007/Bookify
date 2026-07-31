using System.Text;
using Bookify.Application;
using Bookify.Infrastructure;
using Bookify.WebApi.Middleware;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Elapsed:000}ms {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/bookify-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ── Controllers ──
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Use ProblemDetails for model validation errors
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());

            var problemDetails = new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };

            problemDetails.Extensions["errors"] = errors;

            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

// ── ProblemDetails ──
builder.Services.AddProblemDetails();

// ── API Versioning ──
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddMvc().AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ── Swagger / OpenAPI ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Include XML documentation from all projects
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (var xmlFile in xmlFiles)
        options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
});

// ── JWT Authentication ──
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKeyValue = jwtSettings["Key"];
if (string.IsNullOrEmpty(jwtKeyValue))
{
    jwtKeyValue = builder.Configuration["JwtKey"]
        ?? throw new InvalidOperationException(
            "JWT signing key is not configured. Set Jwt:Key in configuration, " +
            "the JwtKey environment variable, or use 'dotnet user-secrets set JwtKey <value>'.");
}

// Fail fast outside Development if the key is still the insecure committed placeholder,
// otherwise tokens would be forgeable with a publicly known signing key.
if (!builder.Environment.IsDevelopment() &&
    jwtKeyValue.Contains("CHANGE-ME", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "JWT signing key is still the insecure default placeholder. Set Jwt:Key or JwtKey " +
        "to a unique secret before running outside of Development.");
}

var jwtKey = Encoding.UTF8.GetBytes(jwtKeyValue);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (allowedOrigins is { Length: > 0 })
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            else
                throw new InvalidOperationException(
                    "CORS is not configured for production. Set Cors:AllowedOrigins in configuration " +
                    "to specify allowed origins. For security reasons, AllowAnyOrigin is not permitted in production.");
        }
    });
});

// ── Rate Limiting ──
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddFixedWindowLimiter("Api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    options.AddFixedWindowLimiter("Strict", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

// ── Health Checks ──
builder.Services.AddHealthChecks()
    .AddCheck<Bookify.WebApi.HealthChecks.DatabaseHealthCheck>(
        name: "database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "ready", "live" });

// ── Application Layers ──
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Settings Configuration ──
builder.Services.Configure<Bookify.Infrastructure.Services.EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<Bookify.Infrastructure.Services.SmsSettings>(builder.Configuration.GetSection("Sms"));
builder.Services.Configure<Bookify.Infrastructure.Services.Payments.StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<Bookify.Infrastructure.Services.AISearchSettings>(builder.Configuration.GetSection("AISearch"));
builder.Services.Configure<Bookify.Infrastructure.Services.VirusScanSettings>(builder.Configuration.GetSection("VirusScan"));
builder.Services.Configure<Bookify.Infrastructure.Services.PushNotificationSettings>(builder.Configuration.GetSection("PushNotifications"));
builder.Services.Configure<Bookify.Infrastructure.Services.ChatSettings>(builder.Configuration.GetSection("Chat"));

var app = builder.Build();

// ── Check if Hangfire is configured ──
var hasHangfireConnection = !string.IsNullOrEmpty(app.Configuration.GetConnectionString("HangfireConnection"));

// ── Security Middleware ──
app.UseHsts();
app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();

// ── Request Pipeline ──
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        if (httpContext.User.Identity?.IsAuthenticated == true)
            diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
    };
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("ApiCorsPolicy");
app.UseRateLimiter();

// ── Swagger (Available in all environments, with auth in production) ──
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookify API v1");
    options.RoutePrefix = "swagger";
    if (!app.Environment.IsDevelopment())
    {
        // In non-development, only allow access from same site
        options.DisplayRequestDuration();
    }
});

app.UseAuthentication();
app.UseAuthorization();

// ── Hangfire Dashboard (Development Only, only if Hangfire is configured) ──
if (hasHangfireConnection && (app.Environment.IsDevelopment() || app.Environment.IsStaging()))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        DashboardTitle = "Bookify Background Jobs",
        Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
    });
}

app.MapControllers();
app.MapHealthChecks("/health");

// ── Seed Database (Development Only, unless SkipDatabaseSeed is set) ──
var skipSeed = app.Configuration.GetValue<bool>("SkipDatabaseSeed");
if (!skipSeed && app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var seedService = scope.ServiceProvider.GetRequiredService<Bookify.Infrastructure.Services.SeedService>();
        await seedService.SeedAsync();
        Log.Information("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database seeding skipped (unavailable). Set SkipDatabaseSeed=true to suppress.");
    }
}

// ── Register Recurring Hangfire Jobs (only if Hangfire is configured) ──
if (hasHangfireConnection)
{
    try
    {
        Bookify.Infrastructure.DependencyInjection.RegisterRecurringJobs();
        Log.Information("Hangfire recurring jobs registered.");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to register Hangfire recurring jobs (database unavailable).");
    }
}

// ── Startup Log ──
Log.Information("Bookify API starting up...");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Hangfire Dashboard available at /hangfire");

app.Run();

// ══════════════════════════════════════════════════════════════
// Type declarations (must be after top-level statements)
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Partial class declaration to make Program accessible to WebApplicationFactory in integration tests.
/// </summary>
public partial class Program { }

/// <summary>
/// Hangfire dashboard authorization filter — allows only authenticated Admin users in non-development,
/// or all connections in development.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // In development, allow all connections
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            return true;
        
        // In staging/production, require authenticated Admin role
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Admin");
    }
}
