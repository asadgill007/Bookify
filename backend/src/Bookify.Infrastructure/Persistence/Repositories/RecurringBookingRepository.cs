using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

public class RecurringBookingRepository : BaseRepository<RecurringBooking>, IRecurringBookingRepository
{
    public RecurringBookingRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<RecurringBooking>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Provider).ThenInclude(p => p.User)
            .Include(r => r.Service)
            .Include(r => r.Business)
            .Where(r => r.ProviderId == providerId && r.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringBooking>> GetByProviderIdPaginatedAsync(Guid providerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Provider).ThenInclude(p => p.User)
            .Include(r => r.Service)
            .Include(r => r.Business)
            .Where(r => r.ProviderId == providerId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(r => r.ProviderId == providerId, cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringBooking>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Provider).ThenInclude(p => p.User)
            .Include(r => r.Service)
            .Include(r => r.Business)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(r => r.CustomerId == customerId, cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringBooking>> GetActiveSeriesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(r => r.Service)
            .Where(r => r.IsActive && !r.HasCompleted
                     && (r.PausedUntil == null || r.PausedUntil <= now))
            .ToListAsync(cancellationToken);
    }
}
