using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasIndex(b => b.Slug)
            .IsUnique()
            .HasFilter("IsDeleted = 0")
            .HasDatabaseName("IX_Businesses_Slug");

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.Property(b => b.Email)
            .HasMaxLength(256);

        builder.Property(b => b.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(b => b.AddressLine1)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.AddressLine2)
            .HasMaxLength(200);

        builder.Property(b => b.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.State)
            .HasMaxLength(100);

        builder.Property(b => b.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Website)
            .HasMaxLength(500);

        builder.Property(b => b.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.VerificationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.VerificationStatus.Pending);

        builder.Property(b => b.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(b => b.ReviewedAt);

        builder.Property(b => b.ReviewedBy);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.BookingType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.BookingType.Instant);

        builder.Property(b => b.CancellationPolicy)
            .HasMaxLength(2000);

        builder.Property(b => b.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("UTC");

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(b => b.AverageRating)
            .IsRequired()
            .HasDefaultValue(0.0)
            .HasColumnType("decimal(2,1)");

        builder.Property(b => b.TotalReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.CoverImageUrl)
            .HasMaxLength(1000);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(1000);

        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Businesses_AverageRating",
            "[AverageRating] >= 0 AND [AverageRating] <= 5"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Businesses_TotalReviews",
            "[TotalReviews] >= 0"));

        // Concurrency token (RowVersion) — skipped on the InMemory provider,
        // which does not support rowversion tokens (see AppDbContext).
        if (!AppDbContext.DisableConcurrencyTokens)
        {
            builder.Property<byte[]>("RowVersion")
                .IsRowVersion()
                .IsConcurrencyToken();
        }

        // Audit fields
        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(b => b.Owner)
            .WithMany()
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Providers)
            .WithOne(p => p.Business)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Services)
            .WithOne(s => s.Business)
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Images)
            .WithOne(i => i.Business)
            .HasForeignKey(i => i.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Reviews)
            .WithOne(r => r.Business)
            .HasForeignKey(r => r.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.BusinessHours)
            .WithOne(h => h.Business)
            .HasForeignKey(h => h.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(b => b.OwnerId)
            .HasDatabaseName("IX_Businesses_OwnerId");

        builder.HasIndex(b => b.AverageRating)
            .IsDescending()
            .HasDatabaseName("IX_Businesses_AverageRating");

        builder.HasIndex(b => new { b.City, b.Country })
            .HasDatabaseName("IX_Businesses_City_Country");

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
