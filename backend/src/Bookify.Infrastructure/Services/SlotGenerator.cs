using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Generates available time slots for a provider considering:
/// - Weekly recurring availability (ProviderAvailability)
/// - Date-specific overrides (holidays, leave, extra hours)
/// - Existing appointments (double booking protection)
/// - Buffer times between appointments
/// - Business hours
/// - Break times via overrides
/// </summary>
public class SlotGenerator : ISlotGenerator
{
    private readonly AppDbContext _context;
    private readonly ILogger<SlotGenerator> _logger;

    public SlotGenerator(AppDbContext context, ILogger<SlotGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TimeSlot>> GenerateSlotsAsync(
        SlotGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var date = request.Date;
        var dayOfWeek = date.DayOfWeek;

        // 1. Get provider's weekly availability for this day of week
        var weeklyAvailability = await _context.ProviderAvailabilities
            .Where(pa => pa.ProviderId == request.ProviderId
                      && pa.DayOfWeek == dayOfWeek
                      && pa.IsAvailable
                      && !pa.IsDeleted)
            .ToListAsync(cancellationToken);

        // 2. Get date-specific overrides for this provider
        var overrides = await _context.ProviderAvailabilityOverrides
            .Where(o => o.ProviderId == request.ProviderId
                     && o.Date == date
                     && !o.IsDeleted)
            .ToListAsync(cancellationToken);

        // 3. Get existing appointments for this provider on this date (non-cancelled)
        var dateStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dateEnd = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var existingAppointments = await _context.Appointments
            .Where(a => a.ProviderId == request.ProviderId
                     && a.StartTime >= dateStart
                     && a.StartTime < dateEnd
                     && a.Status != Domain.Enums.AppointmentStatus.Cancelled
                     && a.Status != Domain.Enums.AppointmentStatus.NoShow
                     && !a.IsDeleted)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        // 4. Check if the entire day is blocked by an override
        var dayOffOverrides = overrides
            .Where(o => !o.IsAvailable && o.StartTime == null && o.EndTime == null)
            .ToList();

        if (dayOffOverrides.Any())
        {
            _logger.LogInformation(
                "Provider {ProviderId} is unavailable on {Date} due to override: {Reason}",
                request.ProviderId, date, dayOffOverrides.First().Reason ?? "Day off");

            return new List<TimeSlot>();
        }

        // 5. Build the effective availability ranges for the day
        var availabilityRanges = BuildAvailabilityRanges(
            weeklyAvailability, overrides, date);

        if (availabilityRanges.Count == 0)
        {
            _logger.LogInformation(
                "No availability found for provider {ProviderId} on {Date}",
                request.ProviderId, date);
            return new List<TimeSlot>();
        }

        // 6. Generate slots from availability ranges, excluding booked times
        var slots = new List<TimeSlot>();
        var bufferMinutes = request.BufferMinutes;
        var slotDuration = request.SlotDurationMinutes;

        foreach (var range in availabilityRanges)
        {
            var currentStart = range.Start;

            while (currentStart.AddMinutes(slotDuration) <= range.End)
            {
                var slotEnd = currentStart.AddMinutes(slotDuration);
                var slotStartUtc = dateStart.Date.Add(currentStart.ToTimeSpan());
                var slotEndUtc = dateStart.Date.Add(slotEnd.ToTimeSpan());

                // Check if this slot conflicts with any existing appointment (with buffer)
                var isBooked = existingAppointments.Any(a =>
                    a.StartTime < slotEndUtc.AddMinutes(bufferMinutes) &&
                    a.EndTime.AddMinutes(bufferMinutes) > slotStartUtc);

                slots.Add(new TimeSlot
                {
                    StartTime = slotStartUtc,
                    EndTime = slotEndUtc,
                    IsAvailable = !isBooked,
                    Reason = isBooked ? "Already booked" : null
                });

                currentStart = currentStart.AddMinutes(slotDuration);
            }
        }

        _logger.LogDebug(
            "Generated {SlotCount} slots for provider {ProviderId} on {Date} ({Available} available)",
            slots.Count, request.ProviderId, date, slots.Count(s => s.IsAvailable));

        return slots;
    }

    private List<TimeRange> BuildAvailabilityRanges(
        List<ProviderAvailability> weeklyAvailability,
        List<ProviderAvailabilityOverride> overrides,
        DateOnly date)
    {
        var ranges = new List<TimeRange>();

        foreach (var availability in weeklyAvailability)
        {
            var range = new TimeRange
            {
                Start = new TimeOnly(availability.StartTime.Hour, availability.StartTime.Minute),
                End = new TimeOnly(availability.EndTime.Hour, availability.EndTime.Minute)
            };

            // Check for date-specific overrides that modify this day
            var dateOverrides = overrides
                .Where(o => o.Date == date)
                .ToList();

            foreach (var ov in dateOverrides)
            {
                if (!ov.IsAvailable)
                {
                    // This specific override blocks availability entirely
                    if (ov.StartTime == null && ov.EndTime == null)
                    {
                        return new List<TimeRange>(); // Full day off
                    }

                    // Partial day off — split the range
                    if (ov.StartTime.HasValue && ov.EndTime.HasValue)
                    {
                        var blockStart = new TimeOnly(ov.StartTime.Value.Hour, ov.StartTime.Value.Minute);
                        var blockEnd = new TimeOnly(ov.EndTime.Value.Hour, ov.EndTime.Value.Minute);

                        // Split the range around the blocked period
                        if (blockStart > range.Start && blockStart < range.End)
                        {
                            ranges.Add(new TimeRange { Start = range.Start, End = blockStart });
                        }
                        if (blockEnd > range.Start && blockEnd < range.End && blockEnd > blockStart)
                        {
                            range = new TimeRange { Start = blockEnd, End = range.End };
                        }
                        else
                        {
                            return ranges; // Range fully blocked
                        }
                    }
                }
                else if (ov.StartTime.HasValue && ov.EndTime.HasValue)
                {
                    // Extended/extra hours override
                    range = new TimeRange
                    {
                        Start = new TimeOnly(ov.StartTime.Value.Hour, ov.StartTime.Value.Minute),
                        End = new TimeOnly(ov.EndTime.Value.Hour, ov.EndTime.Value.Minute)
                    };
                }
            }

            ranges.Add(range);
        }

        // Merge overlapping ranges
        return MergeRanges(ranges);
    }

    private static List<TimeRange> MergeRanges(List<TimeRange> ranges)
    {
        if (ranges.Count <= 1) return ranges;

        var sorted = ranges.OrderBy(r => r.Start).ToList();
        var merged = new List<TimeRange> { sorted[0] };

        for (int i = 1; i < sorted.Count; i++)
        {
            var last = merged[^1];
            var current = sorted[i];

            if (current.Start <= last.End)
            {
                // Overlapping or adjacent — merge
                merged[^1] = new TimeRange
                {
                    Start = last.Start,
                    End = current.End > last.End ? current.End : last.End
                };
            }
            else
            {
                merged.Add(current);
            }
        }

        return merged;
    }

    private struct TimeRange
    {
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
    }
}
