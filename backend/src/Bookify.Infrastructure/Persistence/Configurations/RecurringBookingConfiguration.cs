using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class RecurringBookingConfiguration : IEntityTypeConfiguration<RecurringBooking>
{
    public void Configure(EntityTypeBuilder<RecurringBooking> builder)
    {
        builder.ToTable("RecurringBookings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RecurrenceType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.Interval)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(r => r.DayOfMonth);

        var daysOfWeekConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<List<DayOfWeek>, string>(
            v => string.Join(',', v.Select(d => (int)d)),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => (DayOfWeek)int.Parse(s))
                  .ToList());

        var daysOfWeekComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<DayOfWeek>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(r => r.DaysOfWeek)
            .HasConversion(daysOfWeekConverter, daysOfWeekComparer)
            .HasMaxLength(100);

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        builder.Property(r => r.SeriesStartDate)
            .IsRequired();

        builder.Property(r => r.SeriesEndDate);

        builder.Property(r => r.MaxOccurrences);

        builder.Property(r => r.OccurrencesCreated)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.PausedUntil);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.Provider)
            .WithMany()
            .HasForeignKey(r => r.ProviderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.Service)
            .WithMany()
            .HasForeignKey(r => r.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.Business)
            .WithMany()
            .HasForeignKey(r => r.BusinessId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(r => r.ProviderId);
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => new { r.ProviderId, r.IsActive });

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
