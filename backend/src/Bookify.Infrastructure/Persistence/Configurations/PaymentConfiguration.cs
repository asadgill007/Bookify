using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.PaymentMethod.CreditCard);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.PaymentStatus.Pending);

        builder.Property(p => p.TransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.IsDeposit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.RefundAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.RefundReason)
            .HasMaxLength(500);

        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Payments_Amount",
            "[Amount] >= 0"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Payments_RefundAmount",
            "[RefundAmount] IS NULL OR [RefundAmount] >= 0"));

        builder.HasOne(p => p.Appointment)
            .WithOne(a => a.Payment)
            .HasForeignKey<Payment>(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.AppointmentId)
            .IsUnique()
            .HasDatabaseName("IX_Payments_AppointmentId");

        builder.HasIndex(p => p.TransactionId)
            .HasDatabaseName("IX_Payments_TransactionId");

        builder.HasIndex(p => new { p.CustomerId, p.Status })
            .HasDatabaseName("IX_Payments_CustomerId_Status");

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pt => pt.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(pt => pt.ProviderResponse)
            .HasColumnType("nvarchar(max)");

        builder.Property(pt => pt.IsSuccess)
            .IsRequired()
            .HasDefaultValue(true);

        builder.ToTable(t => t.HasCheckConstraint("CK_PaymentTransactions_Amount",
            "[Amount] >= 0"));

        builder.HasOne(pt => pt.Payment)
            .WithMany(p => p.Transactions)
            .HasForeignKey(pt => pt.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pt => pt.PaymentId)
            .HasDatabaseName("IX_PaymentTransactions_PaymentId");
    }
}
