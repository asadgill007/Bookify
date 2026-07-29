using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.DurationMinutes)
            .IsRequired();

        builder.Property(s => s.PriceAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.PriceCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(s => s.Category)
            .HasMaxLength(100);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Services_DurationMinutes",
            "[DurationMinutes] >= 5 AND [DurationMinutes] <= 1440"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Services_PriceAmount",
            "[PriceAmount] >= 0"));

        builder.HasOne(s => s.Business)
            .WithMany(b => b.Services)
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.BusinessId)
            .HasDatabaseName("IX_Services_BusinessId");

        builder.HasIndex(s => new { s.BusinessId, s.DisplayOrder })
            .HasDatabaseName("IX_Services_BusinessId_DisplayOrder");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
