using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bookify.Application.DTOs.Auth;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bookify.WebApi.Tests.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that configures test-specific settings
/// using EF Core InMemory provider so tests run without a real SQL Server.
/// </summary>
public class BookifyTestApplicationFactory : WebApplicationFactory<Program>
{
    // Unique InMemory database per factory instance so parallel test classes never
    // share (and race on) the same process-wide EF InMemory store.
    private readonly string _dbName = $"BookifyTestDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Skip database seed and Hangfire for tests
        builder.UseSetting("SkipDatabaseSeed", "true");
        builder.UseSetting("ConnectionStrings:HangfireConnection", "");
        builder.UseSetting("Jwt:Key", "SuperSecretTestKeyThatIsAtLeast32CharactersLong!");
        builder.UseSetting("Jwt:Issuer", "BookifyTest");
        builder.UseSetting("Jwt:Audience", "BookifyTestApp");
        builder.UseEnvironment("Development");

        // Use InMemory database for testing
        builder.UseSetting("UseInMemoryDatabase", "true");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _dbName);

        builder.ConfigureServices(services =>
        {
            // Replace the database health check with a trivial one (InMemory doesn't support raw SQL)
            services.RemoveAll(typeof(IHealthCheck));
            services.AddHealthChecks()
                .AddCheck("always_healthy", () => HealthCheckResult.Healthy("Test environment"));
        });
    }
}

/// <summary>
/// Integration tests using WebApplicationFactory with InMemory database.
/// Tests verify the API starts correctly and key endpoints respond properly.
/// </summary>
public class ApiIntegrationTests : IClassFixture<BookifyTestApplicationFactory>
{
    private readonly BookifyTestApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(BookifyTestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5001"),
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Health_Endpoint_ReturnsServiceStatus()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert — accept OK (healthy) or ServiceUnavailable (unhealthy, e.g. DB not connected)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Swagger_Endpoint_ReturnsJson()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert — Swagger JSON should be served
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.RedirectKeepVerb);
    }

    [Fact]
    public async Task Get_NonExistentEndpoint_ReturnsNotFound()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/nonexistent-resource");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unauthenticated_ProtectedEndpoint_ReturnsUnauthorized()
    {
        // Arrange & Act — accessing /api/v1/users/me without auth (route is [HttpGet("me")])
        var response = await _client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Categories_Endpoint_ReturnsOk()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Auth_Register_ReturnsValidationErrors_WhenInvalid()
    {
        // Arrange — invalid registration (missing fields)
        var invalidRequest = new RegisterRequest
        {
            FirstName = "T",
            LastName = "",
            Email = "not-an-email",
            Password = "123",
            ConfirmPassword = "456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", invalidRequest);

        // Assert — should fail with validation error
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Auth_RegisterAndLogin_Flow()
    {
        // Arrange — register a new user
        var registerRequest = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test_{Guid.NewGuid():N}@bookify-test.com",
            Password = "Test@123456!",
            ConfirmPassword = "Test@123456!"
        };

        // Act — register
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert — registration success
        registerResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NoContent);

        // Act — login with the registered credentials
        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert — login succeeds with tokens (response uses the { data: {...} } envelope)
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await loginResponse.Content.ReadFromJsonAsync<AuthEnvelope>();
        envelope.Should().NotBeNull();
        envelope!.Data.Should().NotBeNull();
        envelope.Data!.AccessToken.Should().NotBeNullOrEmpty();
        envelope.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    /// <summary>Typed shape of the ApiResponse envelope returned by the API.</summary>
    private sealed class AuthEnvelope
    {
        public AuthResponse? Data { get; set; }
    }

    [Fact]
    public async Task Appointments_CreateAndVerify_Flow()
    {
        // Arrange — seed provider, business and service directly (customer is registered via API)
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var providerUser = new User("Test", "Provider", $"provider_{Guid.NewGuid():N}@test.com", "hash", UserRole.Provider);
        dbContext.Users.Add(providerUser);
        await dbContext.SaveChangesAsync();

        var business = new Business(providerUser.Id, "Test Business", $"test-business-{Guid.NewGuid():N}", "Test", "Test", "12345", "Test", "UTC");
        dbContext.Businesses.Add(business);
        await dbContext.SaveChangesAsync();

        var provider = new Provider(providerUser.Id, business.Id, "Senior Staff");
        dbContext.Providers.Add(provider);
        await dbContext.SaveChangesAsync();

        var service = new Service(business.Id, "Test Service", 60, 100);
        dbContext.Services.Add(service);
        await dbContext.SaveChangesAsync();

        // Register + login a customer via the API
        var email = $"customer_{Guid.NewGuid():N}@test.com";
        var password = "Test@123456!";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            AccountType = "customer"
        });
        registerResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NoContent);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginEnvelope = await loginResponse.Content.ReadFromJsonAsync<AuthEnvelope>();
        var token = loginEnvelope!.Data!.AccessToken;

        // Act — create an appointment
        var startTime = DateTime.UtcNow.AddDays(3).Date.AddHours(10);
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/appointments")
        {
            Content = JsonContent.Create(new
            {
                ProviderId = provider.Id,
                ServiceId = service.Id,
                BusinessId = business.Id,
                StartTime = startTime,
                EndTime = startTime.AddHours(1)
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Assert — the appointment appears in the user's list
        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/appointments?page=1&pageSize=10");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listResponse = await _client.SendAsync(listRequest);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await listResponse.Content.ReadFromJsonAsync<AppointmentsEnvelope>();
        listEnvelope.Should().NotBeNull();
        listEnvelope!.Data.Should().NotBeNull();
        var item = listEnvelope.Data!.Items.Should().ContainSingle().Subject;
        item.BookingReference.Should().NotBeNullOrEmpty();
        item.Status.Should().Be("Pending");
    }

    /// <summary>Typed shape of the paginated appointments envelope.</summary>
    private sealed class AppointmentsEnvelope
    {
        public AppointmentsData? Data { get; set; }
    }

    private sealed class AppointmentsData
    {
        public List<AppointmentListItem> Items { get; set; } = new();
    }

    private sealed class AppointmentListItem
    {
        public Guid Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
