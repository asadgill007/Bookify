using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.RecurringBookings;

public sealed class GetRecurringBookingsQuery : PagedQuery, IRequest<Result<PaginatedList<RecurringBookingDto>>>
{
    public Guid UserId { get; init; }
    public string Role { get; init; } = "customer";
}

public class RecurringBookingDto
{
    public Guid Id { get; set; }
    public string RecurrenceType { get; set; } = string.Empty;
    public int Interval { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public DateTime SeriesStartDate { get; set; }
    public DateTime? SeriesEndDate { get; set; }
    public int? MaxOccurrences { get; set; }
    public int OccurrencesCreated { get; set; }
    public bool IsActive { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class GetRecurringBookingsQueryHandler : IRequestHandler<GetRecurringBookingsQuery, Result<PaginatedList<RecurringBookingDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRecurringBookingsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<RecurringBookingDto>>> Handle(GetRecurringBookingsQuery request, CancellationToken cancellationToken)
    {
        var isCustomer = request.Role != "provider";

        IReadOnlyList<Domain.Entities.RecurringBooking> bookings;
        int totalCount;

        if (isCustomer)
        {
            bookings = await _unitOfWork.RecurringBookings.GetByCustomerIdAsync(
                request.UserId, request.Page, request.PageSize, cancellationToken);
            totalCount = await _unitOfWork.RecurringBookings.GetCountByCustomerIdAsync(request.UserId, cancellationToken);
        }
        else
        {
            bookings = await _unitOfWork.RecurringBookings.GetByProviderIdPaginatedAsync(
                request.UserId, request.Page, request.PageSize, cancellationToken);
            totalCount = await _unitOfWork.RecurringBookings.GetCountByProviderIdAsync(request.UserId, cancellationToken);
        }

        var items = bookings.Select(b => new RecurringBookingDto
        {
            Id = b.Id,
            RecurrenceType = b.RecurrenceType.ToString(),
            Interval = b.Interval,
            StartTime = b.StartTime.ToString(),
            EndTime = b.EndTime.ToString(),
            SeriesStartDate = b.SeriesStartDate,
            SeriesEndDate = b.SeriesEndDate,
            MaxOccurrences = b.MaxOccurrences,
            OccurrencesCreated = b.OccurrencesCreated,
            IsActive = b.IsActive,
            ProviderName = b.Provider?.User != null ? $"{b.Provider.User.FirstName} {b.Provider.User.LastName}" : "",
            ServiceName = b.Service?.Name ?? "",
            BusinessName = b.Business?.Name ?? "",
            Notes = b.Notes,
            CreatedAt = b.CreatedAt
        }).ToList();

        return Result<PaginatedList<RecurringBookingDto>>.Success(
            new PaginatedList<RecurringBookingDto>(items, request.Page, request.PageSize, totalCount));
    }
}
