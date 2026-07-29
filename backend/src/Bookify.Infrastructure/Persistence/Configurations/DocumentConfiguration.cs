using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Extension)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.FileSize)
            .IsRequired();

        builder.Property(d => d.StoragePath)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(d => d.ThumbnailPath)
            .HasMaxLength(2000);

        builder.Property(d => d.ContentHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.HasOne(d => d.Appointment)
            .WithMany()
            .HasForeignKey(d => d.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Business)
            .WithMany()
            .HasForeignKey(d => d.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Provider)
            .WithMany()
            .HasForeignKey(d => d.ProviderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.UploadedBy)
            .WithMany()
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.BusinessId)
            .HasDatabaseName("IX_Documents_BusinessId");

        builder.HasIndex(d => d.AppointmentId)
            .HasDatabaseName("IX_Documents_AppointmentId");

        builder.HasIndex(d => d.ContentHash)
            .HasDatabaseName("IX_Documents_ContentHash");

        builder.HasIndex(d => new { d.BusinessId, d.DocumentType })
            .HasDatabaseName("IX_Documents_BusinessId_DocumentType");

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
