using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.Property(r => r.IsVerifiedPurchase)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.IsPublished)
            .IsRequired()
            .HasDefaultValue(true);

        // Provider Reply
        builder.Property(r => r.ProviderReply)
            .HasMaxLength(2000);

        builder.Property(r => r.RepliedAt);

        builder.Property(r => r.ReplyUpdatedAt);

        // Moderation
        builder.Property(r => r.IsHidden)
            .HasDefaultValue(false);

        builder.Property(r => r.HiddenAt);

        builder.Property(r => r.HideReason)
            .HasMaxLength(500);

        builder.Property(r => r.ModerationReason)
            .HasMaxLength(500);

        // Check constraint: Rating must be 1-5
        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating",
            "[Rating] >= 1 AND [Rating] <= 5"));

        builder.HasOne(r => r.Appointment)
            .WithOne(a => a.Review)
            .HasForeignKey<Review>(r => r.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Business)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Provider)
            .WithMany()
            .HasForeignKey(r => r.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.AppointmentId)
            .IsUnique()
            .HasDatabaseName("IX_Reviews_AppointmentId");

        builder.HasIndex(r => r.BusinessId)
            .HasDatabaseName("IX_Reviews_BusinessId");

        builder.HasIndex(r => r.CustomerId)
            .HasDatabaseName("IX_Reviews_CustomerId");

        builder.HasIndex(r => r.ProviderId)
            .HasDatabaseName("IX_Reviews_ProviderId");

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
