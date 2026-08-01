using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class FavoriteBusinessConfiguration : IEntityTypeConfiguration<FavoriteBusiness>
{
    public void Configure(EntityTypeBuilder<FavoriteBusiness> builder)
    {
        builder.ToTable("FavoriteBusinesses");

        builder.HasKey(f => f.Id);

        builder.HasIndex(f => new { f.UserId, f.BusinessId })
            .IsUnique()
            .HasDatabaseName("IX_FavoriteBusinesses_UserId_BusinessId");

        builder.Property(f => f.UserId).IsRequired();
        builder.Property(f => f.BusinessId).IsRequired();

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Business)
            .WithMany(b => b.Favorites)
            .HasForeignKey(f => f.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(m => m.Id);

        builder.HasIndex(m => m.UserId)
            .HasDatabaseName("IX_ChatMessages_UserId");

        builder.Property(m => m.Role).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Content).IsRequired().HasMaxLength(4000);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("SupportTickets");

        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("IX_SupportTickets_UserId");

        builder.Property(t => t.Category).IsRequired().HasMaxLength(60);
        builder.Property(t => t.Subject).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Message).IsRequired().HasMaxLength(4000);
        builder.Property(t => t.ContactEmail).HasMaxLength(200);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Appointment)
            .WithMany()
            .HasForeignKey(t => t.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
