using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using MediatR;

namespace Bookify.Application.Queries.Appointments;

public sealed class GetAppointmentsQuery : PagedQuery, IRequest<Result<PaginatedList<AppointmentListItem>>>
{
    public Guid UserId { get; init; }
    public string Role { get; init; } = "customer"; // "customer" or "provider"
    public string? Status { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

public sealed class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, Result<PaginatedList<AppointmentListItem>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<AppointmentListItem>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var isCustomer = request.Role != "provider";

        AppointmentStatus? statusFilter = request.Status?.ToLowerInvariant() switch
        {
            "pending" => AppointmentStatus.Pending,
            "confirmed" => AppointmentStatus.Confirmed,
            "inprogress" => AppointmentStatus.InProgress,
            "completed" => AppointmentStatus.Completed,
            "cancelled" => AppointmentStatus.Cancelled,
            "noshow" => AppointmentStatus.NoShow,
            "rescheduled" => AppointmentStatus.Rescheduled,
            _ => null
        };

        var appointments = await _unitOfWork.Appointments.GetUserAppointmentsAsync(
            request.UserId, isCustomer, statusFilter, request.From, request.To,
            request.Page, request.PageSize, cancellationToken);

        var items = appointments.Select(a => new AppointmentListItem
        {
            Id = a.Id,
            BookingReference = a.BookingReference,
            Status = a.Status.ToString(),
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            TotalAmount = a.TotalAmount,
            Currency = a.Currency,
            ServiceName = a.Service?.Name ?? "",
            ProviderName = a.Provider?.User != null
                ? $"{a.Provider.User.FirstName} {a.Provider.User.LastName}"
                : "",
            ProviderId = a.ProviderId,
            BusinessName = a.Business?.Name ?? "",
            BusinessId = a.BusinessId,
            BusinessAddress = a.Business != null
                ? $"{a.Business.AddressLine1}, {a.Business.City}"
                : "",
            CustomerNotes = a.CustomerNotes,
            CreatedAt = a.CreatedAt
        }).ToList();

        return Result<PaginatedList<AppointmentListItem>>.Success(
            new PaginatedList<AppointmentListItem>(items, request.Page, request.PageSize, items.Count));
    }
}

public class AppointmentListItem
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string ServiceName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public Guid BusinessId { get; set; }
    public string BusinessAddress { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}
