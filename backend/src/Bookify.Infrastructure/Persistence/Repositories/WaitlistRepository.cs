using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

public class WaitlistRepository : BaseRepository<WaitlistEntry>, IWaitlistRepository
{
    public WaitlistRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<WaitlistEntry>> GetBusinessWaitlistAsync(Guid businessId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(w => w.Customer)
            .Include(w => w.Provider).ThenInclude(p => p.User)
            .Include(w => w.Service)
            .Where(w => w.BusinessId == businessId)
            .OrderBy(w => w.Priority).ThenBy(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetBusinessWaitlistCountAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(w => w.BusinessId == businessId, cancellationToken);

    public async Task<IReadOnlyList<WaitlistEntry>> GetProviderWaitlistAsync(Guid providerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(w => w.Customer)
            .Include(w => w.Service)
            .Where(w => w.ProviderId == providerId)
            .OrderBy(w => w.AppointmentDate).ThenBy(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetProviderWaitlistCountAsync(Guid providerId, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(w => w.ProviderId == providerId, cancellationToken);

    public async Task<IReadOnlyList<WaitlistEntry>> GetCustomerWaitlistAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(w => w.Provider).ThenInclude(p => p.User)
            .Include(w => w.Service)
            .Include(w => w.Business)
            .Where(w => w.CustomerId == customerId
                     && (w.Status == WaitlistStatus.Waiting || w.Status == WaitlistStatus.Notified))
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCustomerWaitlistCountAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(w => w.CustomerId == customerId
                                    && (w.Status == WaitlistStatus.Waiting || w.Status == WaitlistStatus.Notified), cancellationToken);

    public async Task<IReadOnlyList<WaitlistEntry>> GetPendingEntriesAsync(Guid providerId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(w => w.Customer)
            .Where(w => w.ProviderId == providerId
                     && w.AppointmentDate == date
                     && w.Status == WaitlistStatus.Waiting)
            .OrderBy(w => w.Priority).ThenBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasDuplicateAsync(Guid customerId, Guid providerId, DateOnly date, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(w => w.CustomerId == customerId
                                  && w.ProviderId == providerId
                                  && w.AppointmentDate == date
                                  && w.Status == WaitlistStatus.Waiting, cancellationToken);

    public async Task<int> GetPositionAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.FirstOrDefaultAsync(w => w.Id == entryId, cancellationToken);
        if (entry == null) return -1;

        var earlier = await DbSet.CountAsync(w => w.ProviderId == entry.ProviderId
                                               && w.AppointmentDate == entry.AppointmentDate
                                               && w.Status == WaitlistStatus.Waiting
                                               && (w.Priority < entry.Priority
                                                   || (w.Priority == entry.Priority && w.CreatedAt < entry.CreatedAt)),
            cancellationToken);

        return earlier + 1;
    }

    public async Task<int> ExpireOldEntriesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expired = await DbSet
            .Where(w => w.Status == WaitlistStatus.Waiting && w.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        foreach (var entry in expired)
            entry.Expire();

        return expired.Count;
    }

    public async Task<WaitlistStatistics> GetStatisticsAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var baseQuery = DbSet.Where(w => w.BusinessId == businessId);

        var totalWaiting = await baseQuery.CountAsync(w => w.Status == WaitlistStatus.Waiting, cancellationToken);
        var totalPromoted = await baseQuery.CountAsync(w => w.Status == WaitlistStatus.Promoted, cancellationToken);
        var totalExpired = await baseQuery.CountAsync(w => w.Status == WaitlistStatus.Expired, cancellationToken);
        var totalCancelled = await baseQuery.CountAsync(w => w.Status == WaitlistStatus.Cancelled, cancellationToken);

        // Calculate average wait time for promoted entries using DB query
        var promotedEntries = await baseQuery
            .Where(w => w.Status == WaitlistStatus.Promoted && w.PromotedAt.HasValue)
            .Select(w => new { w.CreatedAt, PromotedAt = w.PromotedAt!.Value })
            .ToListAsync(cancellationToken);

        var averageWaitDays = promotedEntries.Any()
            ? promotedEntries.Average(e => (e.PromotedAt - e.CreatedAt).TotalDays)
            : 0;

        return new WaitlistStatistics
        {
            TotalWaiting = totalWaiting,
            TotalPromoted = totalPromoted,
            TotalExpired = totalExpired,
            TotalCancelled = totalCancelled,
            AverageWaitDays = Math.Round(averageWaitDays, 1)
        };
    }
}
