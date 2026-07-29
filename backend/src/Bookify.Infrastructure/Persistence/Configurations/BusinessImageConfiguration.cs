using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class BusinessImageConfiguration : IEntityTypeConfiguration<BusinessImage>
{
    public void Configure(EntityTypeBuilder<BusinessImage> builder)
    {
        builder.ToTable("BusinessImages");

        builder.HasKey(bi => bi.Id);

        builder.Property(bi => bi.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(bi => bi.AltText)
            .HasMaxLength(500);

        builder.Property(bi => bi.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(bi => bi.IsCover)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(bi => bi.Business)
            .WithMany(b => b.Images)
            .HasForeignKey(bi => bi.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bi => new { bi.BusinessId, bi.DisplayOrder })
            .HasDatabaseName("IX_BusinessImages_BusinessId_DisplayOrder");

        builder.HasQueryFilter(bi => !bi.IsDeleted);
    }
}

public class BusinessCategoryConfiguration : IEntityTypeConfiguration<BusinessCategory>
{
    public void Configure(EntityTypeBuilder<BusinessCategory> builder)
    {
        builder.ToTable("BusinessCategories");

        builder.HasKey(bc => bc.Id);

        builder.HasIndex(bc => new { bc.BusinessId, bc.CategoryId })
            .IsUnique()
            .HasDatabaseName("IX_BusinessCategories_BusinessId_CategoryId");

        builder.HasOne(bc => bc.Business)
            .WithMany(b => b.BusinessCategories)
            .HasForeignKey(bc => bc.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bc => bc.Category)
            .WithMany(c => c.BusinessCategories)
            .HasForeignKey(bc => bc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(bc => !bc.IsDeleted);
    }
}
