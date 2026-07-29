using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.ToTable("WaitlistEntries");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(WaitlistStatus.Waiting);

        builder.Property(w => w.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(w => w.AppointmentDate)
            .IsRequired();

        builder.Property(w => w.PreferredStartTime);
        builder.Property(w => w.PreferredEndTime);

        builder.Property(w => w.Notes)
            .HasMaxLength(1000);

        builder.Property(w => w.ExpiresAt)
            .IsRequired();

        builder.Property(w => w.NotifiedAt);
        builder.Property(w => w.PromotedAt);

        builder.HasOne(w => w.Business)
            .WithMany()
            .HasForeignKey(w => w.BusinessId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(w => w.Provider)
            .WithMany()
            .HasForeignKey(w => w.ProviderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(w => w.Service)
            .WithMany()
            .HasForeignKey(w => w.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(w => w.Customer)
            .WithMany()
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(w => new { w.ProviderId, w.AppointmentDate, w.Status });
        builder.HasIndex(w => new { w.CustomerId, w.Status });
        builder.HasIndex(w => w.BusinessId);
        builder.HasIndex(w => w.ExpiresAt);

        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
