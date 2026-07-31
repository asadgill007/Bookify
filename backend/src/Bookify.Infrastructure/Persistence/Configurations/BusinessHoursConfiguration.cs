using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class BusinessHoursConfiguration : IEntityTypeConfiguration<BusinessHours>
{
    public void Configure(EntityTypeBuilder<BusinessHours> builder)
    {
        builder.ToTable("BusinessHours");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.DayOfWeek)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.OpenTime)
            .IsRequired();

        builder.Property(h => h.CloseTime)
            .IsRequired();

        builder.Property(h => h.IsClosed)
            .IsRequired()
            .HasDefaultValue(false);

        // One row per day per business
        builder.HasIndex(h => new { h.BusinessId, h.DayOfWeek })
            .IsUnique()
            .HasFilter("IsDeleted = 0")
            .HasDatabaseName("IX_BusinessHours_BusinessId_DayOfWeek");

        builder.HasQueryFilter(h => !h.IsDeleted);
    }
}
