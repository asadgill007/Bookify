using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("IsDeleted = 0")
            .HasDatabaseName("IX_Users_Email");

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.UserRole.Customer);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(1000);

        builder.Property(u => u.IsBiometricEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.LastLoginAt);

        builder.Property(u => u.PreferredLanguage)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(u => u.PreferredCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        // Concurrency token (RowVersion) — skipped on the InMemory provider,
        // which does not support rowversion tokens (see AppDbContext).
        if (!AppDbContext.DisableConcurrencyTokens)
        {
            builder.Property<byte[]>("RowVersion")
                .IsRowVersion()
                .IsConcurrencyToken();
        }

        // Audit fields
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(u => u.CreatedBy);

        builder.Property(u => u.UpdatedBy);

        builder.Property(u => u.DeletedAt);

        builder.Property(u => u.DeletedBy);

        builder.Property(u => u.IsSuspended)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.SuspendedAt);

        builder.Property(u => u.SuspendedBy);

        builder.Property(u => u.SuspensionReason)
            .HasMaxLength(500);

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(u => u.Role)
            .HasDatabaseName("IX_Users_Role");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_Users_CreatedAt");

        // Relationships
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
