using Bookify.Domain.Entities;

namespace Bookify.Application.Interfaces;

public interface IRecurringBookingRepository : IRepository<RecurringBooking>
{
    Task<IReadOnlyList<RecurringBooking>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringBooking>> GetByProviderIdPaginatedAsync(Guid providerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringBooking>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringBooking>> GetActiveSeriesAsync(CancellationToken cancellationToken = default);
}
