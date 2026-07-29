using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class ReviewVoteConfiguration : IEntityTypeConfiguration<ReviewVote>
{
    public void Configure(EntityTypeBuilder<ReviewVote> builder)
    {
        builder.ToTable("ReviewVotes");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.IsHelpful)
            .IsRequired();

        builder.HasOne(v => v.Review)
            .WithMany()
            .HasForeignKey(v => v.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Customer)
            .WithMany()
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.ReviewId, v.CustomerId })
            .IsUnique()
            .HasDatabaseName("IX_ReviewVotes_ReviewId_CustomerId");

        builder.HasIndex(v => v.ReviewId)
            .HasDatabaseName("IX_ReviewVotes_ReviewId");

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
