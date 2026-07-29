using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

public class ProviderRepository : BaseRepository<Provider>, IProviderRepository
{
    public ProviderRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Provider>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.User)
            .Where(p => p.BusinessId == businessId && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAvailabilityAsync(ProviderAvailability availability, CancellationToken cancellationToken = default)
    {
        await Context.Set<ProviderAvailability>().AddAsync(availability, cancellationToken);
    }

    public async Task AddAvailabilityOverrideAsync(ProviderAvailabilityOverride overrideEntry, CancellationToken cancellationToken = default)
    {
        await Context.Set<ProviderAvailabilityOverride>().AddAsync(overrideEntry, cancellationToken);
    }
}

public class ServiceRepository : BaseRepository<Service>, IServiceRepository
{
    public ServiceRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Service>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.BusinessId == businessId && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<Payment?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCustomerPaymentCountAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(p => p.CustomerId == customerId, cancellationToken);
    }
}

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(
        Guid userId, bool? unreadOnly, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(n => n.UserId == userId);

        if (unreadOnly.HasValue && unreadOnly.Value)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = await DbSet.Where(n => n.UserId == userId && !n.IsRead).ToListAsync(cancellationToken);
        foreach (var notification in unread)
        {
            notification.MarkAsRead();
        }
    }
}

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Slug == slug.ToLowerInvariant().Trim(), cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveWithSubCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.SubCategories)
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await DbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
    }

    public async Task<IReadOnlyList<RefreshToken>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.IsRevoked)
            .ToListAsync(cancellationToken);
    }
}
