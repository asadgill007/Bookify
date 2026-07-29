using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(up => up.Id);

        builder.HasIndex(up => up.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserPreferences_UserId");

        builder.Property(up => up.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(up => up.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(up => up.Interests)
            .HasColumnType("nvarchar(max)")
            .HasComment("JSON array of interest IDs");

        builder.Property(up => up.IsDarkMode)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(up => up.IsAmoledMode)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(up => up.NotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(up => up.MarketingEmails)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(up => up.User)
            .WithOne(u => u.Preference)
            .HasForeignKey<UserPreference>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(up => !up.IsDeleted);
    }
}
