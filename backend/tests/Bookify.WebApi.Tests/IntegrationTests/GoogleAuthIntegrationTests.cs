using System.Net;
using System.Net.Http.Json;
using Bookify.Application.DTOs.Auth;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Authentication;
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
/// Factory for Google auth tests: swaps the real JWKS validator for a stub so
/// the whole controller → command → service → response envelope path can be
/// exercised without network calls.
/// </summary>
public class GoogleAuthTestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"BookifyGoogleTestDb_{Guid.NewGuid():N}";

    /// <summary>Behavior applied to every token validation in this factory.</summary>
    public Func<string, CancellationToken, Task<GoogleUserInfo?>> ValidatorBehavior { get; set; } =
        (_, _) => Task.FromResult<GoogleUserInfo?>(null);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("SkipDatabaseSeed", "true");
        builder.UseSetting("ConnectionStrings:HangfireConnection", "");
        builder.UseSetting("Jwt:Key", "SuperSecretTestKeyThatIsAtLeast32CharactersLong!");
        builder.UseSetting("Jwt:Issuer", "BookifyTest");
        builder.UseSetting("Jwt:Audience", "BookifyTestApp");
        builder.UseEnvironment("Development");
        builder.UseSetting("UseInMemoryDatabase", "true");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _dbName);

        builder.ConfigureServices(services =>
        {
            // InMemory database cannot run the real SQL-backed health check.
            services.RemoveAll(typeof(IHealthCheck));
            services.AddHealthChecks()
                .AddCheck("always_healthy", () => HealthCheckResult.Healthy("Test environment"));

            // Replace the Google ID token validator with a deterministic stub.
            services.RemoveAll<IGoogleIdTokenValidator>();
            services.AddScoped<IGoogleIdTokenValidator>(_ => new StubGoogleValidator(ValidatorBehavior));
        });
    }

    private sealed class StubGoogleValidator : IGoogleIdTokenValidator
    {
        private readonly Func<string, CancellationToken, Task<GoogleUserInfo?>> _behavior;

        public StubGoogleValidator(Func<string, CancellationToken, Task<GoogleUserInfo?>> behavior)
            => _behavior = behavior;

        public Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
            => _behavior(idToken, cancellationToken);
    }
}

public class GoogleAuthIntegrationTests : IClassFixture<GoogleAuthTestApplicationFactory>
{
    private readonly GoogleAuthTestApplicationFactory _factory;
    private readonly HttpClient _client;

    public GoogleAuthIntegrationTests(GoogleAuthTestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5001"),
            AllowAutoRedirect = false
        });
    }

    private static GoogleUserInfo VerifiedUser(string email, string subject)
        => new()
        {
            Subject = subject,
            Email = email,
            EmailVerified = true,
            Name = "John Doe",
            Picture = "https://example.com/pic.jpg"
        };

    private sealed class AuthEnvelope
    {
        public AuthResponse? Data { get; set; }
    }

    private sealed class FailEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    [Fact]
    public async Task ValidGoogleToken_CreatesCustomerAndReturnsTokens()
    {
        var email = $"new-google-{Guid.NewGuid():N}@test.com";
        _factory.ValidatorBehavior = (token, _) => Task.FromResult<GoogleUserInfo?>(VerifiedUser(email, "subject-create"));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", new { IdToken = "valid.token", AccountType = "customer" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<AuthEnvelope>();
        envelope!.Data.Should().NotBeNull();
        envelope.Data!.Email.Should().Be(email);
        envelope.Data.Role.Should().Be(UserRole.Customer.ToString());
        envelope.Data.AccessToken.Should().NotBeNullOrEmpty();
        envelope.Data.RefreshToken.Should().NotBeNullOrEmpty();

        // Second sign-in with the same Google identity reuses the account (no duplicate).
        var second = await _client.PostAsJsonAsync("/api/v1/auth/google", new { IdToken = "valid.token", AccountType = "customer" });
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondEnvelope = await second.Content.ReadFromJsonAsync<AuthEnvelope>();
        secondEnvelope!.Data!.UserId.Should().Be(envelope.Data.UserId);
    }

    [Fact]
    public async Task ValidGoogleToken_ExistingLocalAccount_IsLinkedNotDuplicated()
    {
        var email = $"link-google-{Guid.NewGuid():N}@test.com";
        _factory.ValidatorBehavior = (token, _) => Task.FromResult<GoogleUserInfo?>(VerifiedUser(email, "subject-link"));

        // Seed an existing local account with the same email.
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            dbContext.Users.Add(new User("Local", "User", email, "hash", UserRole.Customer));
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", new { IdToken = "valid.token" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<AuthEnvelope>();
        envelope!.Data!.Email.Should().Be(email);

        // Exactly one user exists and it is the linked local account.
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = await verifyDb.Users.Where(u => u.Email == email).ToListAsync();
        users.Should().ContainSingle();
        users[0].GoogleSubject.Should().Be("subject-link");
    }

    [Fact]
    public async Task InvalidGoogleToken_ReturnsBadRequest()
    {
        // Validator rejects the token (invalid/expired/signature-failure cases).
        _factory.ValidatorBehavior = (_, _) => Task.FromResult<GoogleUserInfo?>(null);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", new { IdToken = "garbage.token" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await response.Content.ReadFromJsonAsync<FailEnvelope>();
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Contain("could not verify");
    }

    [Fact]
    public async Task UnverifiedEmail_ReturnsBadRequest()
    {
        _factory.ValidatorBehavior = (_, _) => Task.FromResult<GoogleUserInfo?>(new GoogleUserInfo
        {
            Subject = "subject-unverified",
            Email = $"unverified-{Guid.NewGuid():N}@test.com",
            EmailVerified = false,
            Name = "John Doe"
        });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", new { IdToken = "valid.token" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await response.Content.ReadFromJsonAsync<FailEnvelope>();
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Contain("not verified");
    }

    [Fact]
    public async Task MissingIdToken_ReturnsValidationError()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", new { IdToken = "" });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
