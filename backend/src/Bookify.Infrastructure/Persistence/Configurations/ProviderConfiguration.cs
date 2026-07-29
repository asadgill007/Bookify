using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(200);

        builder.Property(p => p.Bio)
            .HasMaxLength(2000);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Business)
            .WithMany(b => b.Providers)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Providers_UserId");

        builder.HasIndex(p => p.BusinessId)
            .HasDatabaseName("IX_Providers_BusinessId");

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

public class ProviderServiceConfiguration : IEntityTypeConfiguration<ProviderService>
{
    public void Configure(EntityTypeBuilder<ProviderService> builder)
    {
        builder.ToTable("ProviderServices");

        builder.HasKey(ps => ps.Id);

        builder.HasIndex(ps => new { ps.ProviderId, ps.ServiceId })
            .IsUnique()
            .HasDatabaseName("IX_ProviderServices_ProviderId_ServiceId");

        builder.HasOne(ps => ps.Provider)
            .WithMany(p => p.ProviderServices)
            .HasForeignKey(ps => ps.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.Service)
            .WithMany(s => s.ProviderServices)
            .HasForeignKey(ps => ps.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(ps => !ps.IsDeleted);
    }
}

public class ProviderAvailabilityConfiguration : IEntityTypeConfiguration<ProviderAvailability>
{
    public void Configure(EntityTypeBuilder<ProviderAvailability> builder)
    {
        builder.ToTable("ProviderAvailabilities");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.DayOfWeek)
            .IsRequired();

        builder.Property(pa => pa.StartTime)
            .IsRequired();

        builder.Property(pa => pa.EndTime)
            .IsRequired();

        builder.Property(pa => pa.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pa => pa.SlotDurationMinutes)
            .IsRequired()
            .HasDefaultValue(60);

        // Check constraint for valid slot duration
        builder.ToTable(t => t.HasCheckConstraint("CK_ProviderAvailabilities_SlotDuration",
            "[SlotDurationMinutes] >= 15 AND [SlotDurationMinutes] <= 480"));

        // Check constraint: EndTime > StartTime
        builder.ToTable(t => t.HasCheckConstraint("CK_ProviderAvailabilities_TimeRange",
            "[EndTime] > [StartTime]"));

        builder.HasOne(pa => pa.Provider)
            .WithMany(p => p.Availabilities)
            .HasForeignKey(pa => pa.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pa => new { pa.ProviderId, pa.DayOfWeek })
            .HasDatabaseName("IX_ProviderAvailabilities_ProviderId_DayOfWeek");

        builder.HasQueryFilter(pa => !pa.IsDeleted);
    }
}

public class ProviderAvailabilityOverrideConfiguration : IEntityTypeConfiguration<ProviderAvailabilityOverride>
{
    public void Configure(EntityTypeBuilder<ProviderAvailabilityOverride> builder)
    {
        builder.ToTable("ProviderAvailabilityOverrides");

        builder.HasKey(pao => pao.Id);

        builder.Property(pao => pao.Date)
            .IsRequired();

        builder.Property(pao => pao.IsAvailable)
            .IsRequired();

        builder.Property(pao => pao.Reason)
            .HasMaxLength(500);

        builder.HasOne(pao => pao.Provider)
            .WithMany(p => p.AvailabilityOverrides)
            .HasForeignKey(pao => pao.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pao => new { pao.ProviderId, pao.Date })
            .IsUnique()
            .HasDatabaseName("IX_AvailabilityOverrides_ProviderId_Date");

        builder.HasQueryFilter(pao => !pao.IsDeleted);
    }
}
