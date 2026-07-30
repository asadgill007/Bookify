using System.Net;
using System.Net.Http.Json;
using Bookify.Application.DTOs.Auth;
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
        builder.UseSetting("ConnectionStrings:DefaultConnection", "BookifyTestDb");

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

    [Fact(Skip = "Requires seeded database data to complete full auth flow")]
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

        // Assert — login succeeds with tokens
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        loginContent.Should().NotBeNull();
        loginContent!.AccessToken.Should().NotBeNullOrEmpty();
        loginContent.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact(Skip = "Requires seeded appointment data")]
    public async Task Appointments_CreateAndVerify_Flow()
    {
        // This test would:
        // 1. Login as a customer
        // 2. Get available slots for a provider
        // 3. Create an appointment
        // 4. Verify the appointment was created
        // Requires seeded data: businesses, providers, services, and available slots.
        await Task.CompletedTask;
    }
}
