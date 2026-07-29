using Bookify.Domain.Entities;

namespace Bookify.Application.Interfaces;

public interface IWaitlistRepository : IRepository<WaitlistEntry>
{
    Task<IReadOnlyList<WaitlistEntry>> GetBusinessWaitlistAsync(Guid businessId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetBusinessWaitlistCountAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WaitlistEntry>> GetProviderWaitlistAsync(Guid providerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetProviderWaitlistCountAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WaitlistEntry>> GetCustomerWaitlistAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCustomerWaitlistCountAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WaitlistEntry>> GetPendingEntriesAsync(Guid providerId, DateOnly date, CancellationToken cancellationToken = default);
    Task<bool> HasDuplicateAsync(Guid customerId, Guid providerId, DateOnly date, CancellationToken cancellationToken = default);
    Task<int> GetPositionAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task<int> ExpireOldEntriesAsync(CancellationToken cancellationToken = default);
    Task<WaitlistStatistics> GetStatisticsAsync(Guid businessId, CancellationToken cancellationToken = default);
}

public class WaitlistStatistics
{
    public int TotalWaiting { get; set; }
    public int TotalPromoted { get; set; }
    public int TotalExpired { get; set; }
    public int TotalCancelled { get; set; }
    public double AverageWaitDays { get; set; }
}
