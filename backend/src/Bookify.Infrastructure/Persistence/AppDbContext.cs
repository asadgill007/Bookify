using System.Reflection;
using Bookify.Domain.Common;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    /// <summary>
    /// True when running on the EF Core InMemory provider. The InMemory provider
    /// does not support SQL Server rowversion concurrency tokens — updating an
    /// entity that has one throws a spurious DbUpdateConcurrencyException even
    /// though the write succeeds. When true, rowversion tokens are not
    /// configured, so dev/test flows work while SQL Server keeps optimistic
    /// concurrency. Set once at startup from the UseInMemoryDatabase setting.
    /// </summary>
    public static bool DisableConcurrencyTokens { get; set; }

    private readonly ICurrentUserService? _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        // Defensive fallback: also detect the InMemory provider here so contexts
        // constructed outside DI (e.g. tests) skip rowversion tokens too.
        if (Database.IsInMemory())
        {
            DisableConcurrencyTokens = true;
        }
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;

        if (Database.IsInMemory())
        {
            DisableConcurrencyTokens = true;
        }
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

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex) when (Database.IsInMemory())
        {
            // InMemory provider quirk: when a status transition adds new
            // dependents (e.g. AppointmentLog) whose FK points at an already-
            // loaded principal, relationship fixup can track them as Modified
            // instead of Added (verified: the new dependent is Modified before
            // SaveChanges even runs). Their keys are not yet in the store, so
            // InMemoryTable.Update throws "Attempted to update or delete an
            // entity that does not exist in the store." Such entries are genuine
            // INSERTs — restore them to Added and retry. This only ever triggers
            // on the InMemory provider (SQL Server is untouched) and there is no
            // real optimistic concurrency in-memory, so restoring the state is
            // always semantically correct.
            //
            // Multiple dependents can be mis-tracked in one batch (cancelling a
            // recurring series adds one log per occurrence), so retry in a
            // bounded loop. Each attempt persists the entry that failed before
            // (the failure point moves forward through the batch), so entries
            // flipped in earlier attempts are already in the store and must be
            // marked Unchanged before the next attempt to avoid duplicate
            // INSERTs.
            var flippedToAdd = new HashSet<object>();
            for (var attempt = 0; attempt < 50; attempt++)
            {
                // Entries flipped in earlier attempts were persisted by those
                // attempts; accept them so the retry doesn't INSERT them again.
                foreach (var entity in flippedToAdd)
                {
                    var entry = Entry(entity);
                    if (entry.State == EntityState.Added)
                    {
                        entry.State = EntityState.Unchanged;
                    }
                }

                // The currently-failing entry is a genuine INSERT that was
                // mis-tracked as Modified; restore it to Added.
                foreach (var entry in ex.Entries)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        entry.State = EntityState.Added;
                        flippedToAdd.Add(entry.Entity);
                    }
                }

                try
                {
                    return await base.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException retryEx) when (Database.IsInMemory())
                {
                    ex = retryEx;
                }
            }

            throw;
        }
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
