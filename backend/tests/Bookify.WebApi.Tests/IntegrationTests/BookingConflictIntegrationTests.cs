using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bookify.WebApi.Tests.IntegrationTests;

public class BookingConflictIntegrationTests : IClassFixture<BookifyTestApplicationFactory>
{
    private readonly BookifyTestApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private readonly AppDbContext _dbContext;

    public BookingConflictIntegrationTests(BookifyTestApplicationFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [Fact]
    public async Task CreateAppointment_WithTimeConflict_ReturnsBadRequest()
    {
        // Arrange — seed provider, business and service (customer is registered via API)
        await _dbContext.Database.EnsureCreatedAsync();

        var providerUser = new User("Test", "Provider", $"provider_{Guid.NewGuid():N}@test.com", "hash", UserRole.Provider);
        _dbContext.Users.Add(providerUser);
        await _dbContext.SaveChangesAsync();

        var business = new Business(providerUser.Id, "Test Business", $"test-business-{Guid.NewGuid():N}", "Test", "Test", "12345", "Test", "UTC");
        _dbContext.Businesses.Add(business);
        await _dbContext.SaveChangesAsync();

        var provider = new Provider(providerUser.Id, business.Id, "Senior Staff");
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var service = new Service(business.Id, "Test Service", 60, 100);
        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        var token = await RegisterAndLoginAsync(client);

        var startTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var endTime = startTime.AddHours(1);

        // Act — first appointment succeeds
        var appointment1 = new
        {
            ProviderId = provider.Id,
            ServiceId = service.Id,
            BusinessId = business.Id,
            StartTime = startTime,
            EndTime = endTime
        };
        var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/appointments")
        {
            Content = JsonContent.Create(appointment1)
        };
        request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response1 = await client.SendAsync(request1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act — overlapping appointment for the same provider must be rejected
        var appointment2 = new
        {
            ProviderId = provider.Id,
            ServiceId = service.Id,
            BusinessId = business.Id,
            StartTime = startTime.AddMinutes(30),
            EndTime = endTime.AddMinutes(30)
        };
        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/appointments")
        {
            Content = JsonContent.Create(appointment2)
        };
        request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response2 = await client.SendAsync(request2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAppointment_WithNoConflict_ReturnsSuccess()
    {
        // Arrange
        await _dbContext.Database.EnsureCreatedAsync();

        var providerUser = new User("Test", "Provider", $"provider2_{Guid.NewGuid():N}@test.com", "hash", UserRole.Provider);
        _dbContext.Users.Add(providerUser);
        await _dbContext.SaveChangesAsync();

        var business = new Business(providerUser.Id, "Test Business 2", $"test-business-2-{Guid.NewGuid():N}", "Test", "Test", "12345", "Test", "UTC");
        _dbContext.Businesses.Add(business);
        await _dbContext.SaveChangesAsync();

        var provider = new Provider(providerUser.Id, business.Id, "Senior Staff");
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var service = new Service(business.Id, "Test Service 2", 60, 100);
        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();
        var token = await RegisterAndLoginAsync(client);

        var startTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endTime = startTime.AddHours(1);

        // Act
        var appointment = new
        {
            ProviderId = provider.Id,
            ServiceId = service.Id,
            BusinessId = business.Id,
            StartTime = startTime,
            EndTime = endTime
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/appointments")
        {
            Content = JsonContent.Create(appointment)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client)
    {
        var email = $"customer_{Guid.NewGuid():N}@test.com";
        var password = "Test@123456!";

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
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

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = email,
            Password = password
        });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await loginResponse.Content.ReadFromJsonAsync<LoginEnvelope>();
        envelope?.Data?.AccessToken.Should().NotBeNullOrEmpty();
        return envelope!.Data!.AccessToken;
    }

    private sealed class LoginEnvelope
    {
        public LoginData? Data { get; set; }
    }

    private sealed class LoginData
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}