using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
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

    [Fact(Skip = "Requires authentication setup in test infrastructure")]
    public async Task CreateAppointment_WithTimeConflict_ReturnsBadRequest()
    {
        // Arrange
        await _dbContext.Database.EnsureCreatedAsync();
        
        var customerId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        // Create test data using correct constructors
        var customer = new User("Test", "Customer", "customer@test.com", "hash", UserRole.Customer);
        var provider = new User("Test", "Provider", "provider@test.com", "hash", UserRole.Provider);
        var business = new Business(providerId, "Test Business", "test-business", "Test", "Test", "12345", "Test", "UTC");
        var service = new Service(businessId, "Test Service", 60, 100);

        // Set IDs using reflection
        SetEntityId(customer, customerId);
        SetEntityId(provider, providerId);
        SetEntityId(business, businessId);
        SetEntityId(service, serviceId);

        _dbContext.Users.AddRange(customer, provider);
        _dbContext.Businesses.Add(business);
        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var endTime = startTime.AddHours(1);

        // Create first appointment using correct constructor
        var appointment1 = new Appointment(
            "REF001",
            customerId,
            providerId,
            serviceId,
            businessId,
            startTime,
            endTime,
            100,
            "USD");

        _dbContext.Appointments.Add(appointment1);
        await _dbContext.SaveChangesAsync();

        // Act - Try to create conflicting appointment
        var client = _factory.CreateClient();
        var conflictingAppointment = new
        {
            CustomerId = customerId,
            ProviderId = providerId,
            ServiceId = serviceId,
            BusinessId = businessId,
            StartTime = startTime.AddMinutes(30), // Overlaps with first appointment
            EndTime = endTime.AddMinutes(30)
        };

        var response = await client.PostAsJsonAsync("/api/appointments", conflictingAppointment);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(Skip = "Requires authentication setup in test infrastructure")]
    public async Task CreateAppointment_WithNoConflict_ReturnsSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var customer = new User("Test", "Customer", "customer2@test.com", "hash", UserRole.Customer);
        var provider = new User("Test", "Provider", "provider2@test.com", "hash", UserRole.Provider);
        var business = new Business(providerId, "Test Business 2", "test-business-2", "Test", "Test", "12345", "Test", "UTC");
        var service = new Service(businessId, "Test Service 2", 60, 100);

        SetEntityId(customer, customerId);
        SetEntityId(provider, providerId);
        SetEntityId(business, businessId);
        SetEntityId(service, serviceId);

        _dbContext.Users.AddRange(customer, provider);
        _dbContext.Businesses.Add(business);
        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endTime = startTime.AddHours(1);

        // Act
        var client = _factory.CreateClient();
        var appointment = new
        {
            CustomerId = customerId,
            ProviderId = providerId,
            ServiceId = serviceId,
            BusinessId = businessId,
            StartTime = startTime,
            EndTime = endTime
        };

        var response = await client.PostAsJsonAsync("/api/appointments", appointment);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static void SetEntityId(object entity, Guid id)
    {
        entity.GetType().GetProperty("Id")?.SetValue(entity, id);
    }
}