using System.Reflection;
using Bookify.Domain.Common;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<BusinessCategory> BusinessCategories => Set<BusinessCategory>();
    public DbSet<BusinessImage> BusinessImages => Set<BusinessImage>();
    public DbSet<BusinessHours> BusinessHours => Set<BusinessHours>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderService> ProviderServices => Set<ProviderService>();
    public DbSet<ProviderAvailability> ProviderAvailabilities => Set<ProviderAvailability>();
    public DbSet<ProviderAvailabilityOverride> ProviderAvailabilityOverrides => Set<ProviderAvailabilityOverride>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentLog> AppointmentLogs => Set<AppointmentLog>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewVote> ReviewVotes => Set<ReviewVote>();
    public DbSet<ReviewReport> ReviewReports => Set<ReviewReport>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RecurringBooking> RecurringBookings => Set<RecurringBooking>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filters for soft deletes on all entities with IsDeleted
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var isDeletedProp = entityType.FindProperty("IsDeleted");
            if (isDeletedProp != null && isDeletedProp.ClrType == typeof(bool))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(
                        System.Linq.Expressions.Expression.Property(parameter, "IsDeleted"),
                        System.Linq.Expressions.Expression.Constant(false)),
                    parameter);
                entityType.SetQueryFilter(filter);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService?.GetCurrentUserId();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (currentUserId.HasValue)
                    {
                        entry.Entity.SetCreatedBy(currentUserId.Value);
                        entry.Entity.SetUpdatedBy(currentUserId.Value);
                    }
                    entry.Entity.Touch(currentUserId);
                    break;

                case EntityState.Modified:
                    if (currentUserId.HasValue)
                        entry.Entity.SetUpdatedBy(currentUserId.Value);
                    entry.Entity.Touch(currentUserId);
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete(currentUserId);
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Service to get the current user's ID from the HTTP context.
/// Injected into DbContext for automatic audit field population.
/// </summary>
public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
}
