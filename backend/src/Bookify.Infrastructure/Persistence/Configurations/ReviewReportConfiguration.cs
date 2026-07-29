using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class ReviewReportConfiguration : IEntityTypeConfiguration<ReviewReport>
{
    public void Configure(EntityTypeBuilder<ReviewReport> builder)
    {
        builder.ToTable("ReviewReports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ReportStatus.Pending);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.Resolution)
            .HasMaxLength(500);

        builder.HasOne(r => r.Review)
            .WithMany()
            .HasForeignKey(r => r.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReportedBy)
            .WithMany()
            .HasForeignKey(r => r.ReportedByCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.ReviewId)
            .HasDatabaseName("IX_ReviewReports_ReviewId");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_ReviewReports_Status");

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
