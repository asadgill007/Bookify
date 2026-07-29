using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.BookingReference)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => a.BookingReference)
            .IsUnique()
            .HasDatabaseName("IX_Appointments_BookingReference");

        builder.Property(a => a.CustomerNotes)
            .HasMaxLength(1000);

        builder.Property(a => a.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(a => a.CancellationReason)
            .HasMaxLength(500);

        builder.Property(a => a.IsCustomerNotified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.AppointmentStatus.Pending);

        // Check constraint: EndTime > StartTime
        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_TimeRange",
            "[EndTime] > [StartTime]"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_TotalAmount",
            "[TotalAmount] >= 0"));

        // Concurrency token
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken();

        // Audit fields
        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Provider)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Business)
            .WithMany()
            .HasForeignKey(a => a.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.RescheduledFrom)
            .WithMany()
            .HasForeignKey(a => a.RescheduledFromId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(a => a.Logs)
            .WithOne(l => l.Appointment)
            .HasForeignKey(l => l.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.CustomerId)
            .HasDatabaseName("IX_Appointments_CustomerId");

        builder.HasIndex(a => a.ProviderId)
            .HasDatabaseName("IX_Appointments_ProviderId");

        builder.HasIndex(a => a.StartTime)
            .HasDatabaseName("IX_Appointments_StartTime");

        builder.HasIndex(a => new { a.BusinessId, a.StartTime })
            .HasDatabaseName("IX_Appointments_BusinessId_StartTime");

        builder.HasIndex(a => new { a.ProviderId, a.StartTime })
            .HasDatabaseName("IX_Appointments_ProviderId_StartTime");

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

public class AppointmentLogConfiguration : IEntityTypeConfiguration<AppointmentLog>
{
    public void Configure(EntityTypeBuilder<AppointmentLog> builder)
    {
        builder.ToTable("AppointmentLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.FromStatus);

        builder.Property(l => l.ToStatus)
            .IsRequired();

        builder.Property(l => l.Reason)
            .HasMaxLength(500);

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(l => l.Appointment)
            .WithMany(a => a.Logs)
            .HasForeignKey(l => l.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.AppointmentId)
            .HasDatabaseName("IX_AppointmentLogs_AppointmentId");
    }
}
