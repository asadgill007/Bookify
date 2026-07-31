using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Tests;

/// <summary>
/// Regression tests for the EF Core InMemory provider behaviors that previously
/// broke dev/test flows:
///   1. RowVersion concurrency tokens must not be configured when InMemory
///      (AppDbContext.DisableConcurrencyTokens).
///   2. Adding a new dependent (AppointmentLog) during a status transition on a
///      loaded principal must save cleanly — relationship fixup can mis-track
///      the new dependent as Modified, which made InMemoryTable.Update throw
///      "entity does not exist in the store".
/// </summary>
public class InMemoryRegressionTests
{
    [Fact]
    public void Model_WithFlagTrue_HasNoRowVersionOnAppointment()
    {
        AppDbContext.DisableConcurrencyTokens = true;
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"reg_model_{Guid.NewGuid():N}")
            .Options;

        using var ctx = new AppDbContext(options);
        var model = ctx.Model;
        var appt = model.FindEntityType(typeof(Appointment));
        var rowVersion = appt?.FindProperty("RowVersion");
        var business = model.FindEntityType(typeof(Business));
        var businessRv = business?.FindProperty("RowVersion");
        var user = model.FindEntityType(typeof(User));
        var userRv = user?.FindProperty("RowVersion");

        // With the flag set, these shadow rowversion properties must not exist.
        rowVersion.Should().BeNull($"Appointment RowVersion still configured: {rowVersion}");
        businessRv.Should().BeNull($"Business RowVersion still configured: {businessRv}");
        userRv.Should().BeNull($"User RowVersion still configured: {userRv}");
    }

    [Fact]
    public async Task ConfirmAppointment_InMemory_DoesNotThrow()
    {
        AppDbContext.DisableConcurrencyTokens = true;
        var dbName = $"reg_confirm_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        Guid apptId;
        // Seed
        using (var ctx = new AppDbContext(options))
        {
            var providerUser = new User("Test", "Provider", $"p_{Guid.NewGuid():N}@test.com", "hash", UserRole.Provider);
            ctx.Users.Add(providerUser);
            await ctx.SaveChangesAsync();

            var business = new Business(providerUser.Id, "Reg Biz", $"reg-biz-{Guid.NewGuid():N}", "T", "T", "12345", "T", "UTC");
            ctx.Businesses.Add(business);
            await ctx.SaveChangesAsync();

            var provider = new Provider(providerUser.Id, business.Id, "Senior");
            ctx.Providers.Add(provider);
            await ctx.SaveChangesAsync();

            var service = new Service(business.Id, "Reg Service", 60, 100);
            ctx.Services.Add(service);
            await ctx.SaveChangesAsync();

            var start = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
            var appt = new Appointment(
                "BOK-REG001", Guid.NewGuid(), provider.Id, service.Id, business.Id, start, start.AddHours(1), 100);
            ctx.Appointments.Add(appt);
            await ctx.SaveChangesAsync();
            apptId = appt.Id;
        }

        // New context (new request) → load → Confirm → save
        await using var ctx2 = new AppDbContext(options);
        var loaded = await ctx2.Appointments.FirstOrDefaultAsync(a => a.Id == apptId);
        loaded.Should().NotBeNull();

        loaded!.Confirm();
        await ctx2.SaveChangesAsync();
        loaded.Status.Should().Be(AppointmentStatus.Confirmed);

        // The new log must actually be persisted too.
        var log = await ctx2.AppointmentLogs
            .FirstOrDefaultAsync(l => l.AppointmentId == apptId && l.ToStatus == AppointmentStatus.Confirmed);
        log.Should().NotBeNull("the 'Appointment confirmed' log should be inserted");
    }

    [Fact]
    public async Task CancelSeries_MultipleDependents_AllPersist()
    {
        // Regression for the cancel-series path: cancelling a recurring series
        // modifies the series AND N future appointments in one save, each adding
        // a new AppointmentLog. Relationship fixup mis-tracks each new dependent
        // as Modified; the InMemory retry must recover all of them, not just one.
        AppDbContext.DisableConcurrencyTokens = true;
        var dbName = $"reg_cancel_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var seriesId = Guid.NewGuid();
        var apptIds = new List<Guid>();

        // Seed
        using (var ctx = new AppDbContext(options))
        {
            var providerUser = new User("Test", "Provider", $"cp_{Guid.NewGuid():N}@test.com", "hash", UserRole.Provider);
            ctx.Users.Add(providerUser);
            await ctx.SaveChangesAsync();

            var business = new Business(providerUser.Id, "Cancel Biz", $"cancel-biz-{Guid.NewGuid():N}", "T", "T", "12345", "T", "UTC");
            ctx.Businesses.Add(business);
            await ctx.SaveChangesAsync();

            var provider = new Provider(providerUser.Id, business.Id, "Senior");
            ctx.Providers.Add(provider);
            await ctx.SaveChangesAsync();

            var service = new Service(business.Id, "Cancel Service", 60, 100);
            ctx.Services.Add(service);
            await ctx.SaveChangesAsync();

            var customerId = Guid.NewGuid();
            var series = new RecurringBooking(
                customerId, provider.Id, service.Id, business.Id,
                RecurrenceType.Weekly,
                new TimeOnly(10, 0), new TimeOnly(11, 0),
                DateTime.UtcNow.AddDays(7),
                maxOccurrences: 4,
                daysOfWeek: new List<DayOfWeek> { DayOfWeek.Monday });
            ctx.RecurringBookings.Add(series);
            await ctx.SaveChangesAsync();
            seriesId = series.Id;

            for (var i = 0; i < 4; i++)
            {
                var start = DateTime.UtcNow.AddDays(7 + (7 * i)).Date.AddHours(10);
                var appt = new Appointment(
                    $"BOK-CAN{i:000}", customerId, provider.Id, service.Id, business.Id,
                    start, start.AddHours(1), 100);
                appt.AttachToRecurringSeries(seriesId);
                ctx.Appointments.Add(appt);
                apptIds.Add(appt.Id);
            }
            await ctx.SaveChangesAsync();
        }

        // New request: load the series and its future occurrences, cancel them all.
        await using var ctx2 = new AppDbContext(options);
        var loadedSeries = await ctx2.RecurringBookings.FirstOrDefaultAsync(r => r.Id == seriesId);
        loadedSeries.Should().NotBeNull();
        loadedSeries!.CancelSeries();

        var future = await ctx2.Appointments
            .Where(a => a.RecurringBookingId == seriesId && a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();
        future.Should().HaveCount(4);
        foreach (var appt in future)
        {
            appt.Cancel("Recurring series cancelled");
        }

        await ctx2.SaveChangesAsync();

        // Every occurrence must be cancelled and every log persisted.
        var cancelled = await ctx2.Appointments.CountAsync(a => a.RecurringBookingId == seriesId && a.Status == AppointmentStatus.Cancelled);
        cancelled.Should().Be(4, "all future occurrences should be cancelled");
        var logs = await ctx2.AppointmentLogs.CountAsync(l => l.Reason == "Recurring series cancelled");
        logs.Should().Be(4, "each cancelled occurrence should produce a log");
    }
}
