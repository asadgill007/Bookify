using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using MediatR;

namespace Bookify.Application.Queries.Appointments;

public sealed record GetAppointmentByIdQuery : IRequest<Result<AppointmentDetail>>
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDetail>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AppointmentDetail>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result<AppointmentDetail>.Failure("Appointment not found.", "NOT_FOUND");

        // Verify the user has access to this appointment
        var isOwner = appointment.CustomerId == request.UserId;
        var isProvider = appointment.Provider?.UserId == request.UserId;
        var isBusinessOwner = appointment.Business?.OwnerId == request.UserId;

        if (!isOwner && !isProvider && !isBusinessOwner)
            return Result<AppointmentDetail>.Failure("You do not have access to this appointment.", "FORBIDDEN");

        var detail = new AppointmentDetail
        {
            Id = appointment.Id,
            BookingReference = appointment.BookingReference,
            Status = appointment.Status.ToString(),
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            TotalAmount = appointment.TotalAmount,
            Currency = appointment.Currency,
            CustomerNotes = appointment.CustomerNotes,
            CancellationReason = appointment.CancellationReason,
            IsCustomerNotified = appointment.IsCustomerNotified,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt,
            ServiceName = appointment.Service?.Name ?? "",
            ServiceId = appointment.ServiceId,
            ServiceDurationMinutes = appointment.Service?.DurationMinutes ?? 0,
            ProviderName = appointment.Provider?.User != null
                ? $"{appointment.Provider.User.FirstName} {appointment.Provider.User.LastName}"
                : "",
            ProviderId = appointment.ProviderId,
            BusinessName = appointment.Business?.Name ?? "",
            BusinessId = appointment.BusinessId,
            BusinessAddress = appointment.Business != null
                ? $"{appointment.Business.AddressLine1}, {appointment.Business.City}, {appointment.Business.Country}"
                : "",
            CustomerName = appointment.Customer != null
                ? $"{appointment.Customer.FirstName} {appointment.Customer.LastName}"
                : "",
            CustomerId = appointment.CustomerId,
            HasReview = appointment.Review != null,
            HasPayment = appointment.Payment != null,
            StatusHistory = appointment.Logs.Select(log => new AppointmentStatusLogEntry
            {
                FromStatus = log.FromStatus?.ToString(),
                ToStatus = log.ToStatus.ToString(),
                Reason = log.Reason,
                Timestamp = log.CreatedAt
            }).ToList()
        };

        return Result<AppointmentDetail>.Success(detail);
    }
}

public class AppointmentDetail
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? CustomerNotes { get; set; }
    public string? CancellationReason { get; set; }
    public bool IsCustomerNotified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string ServiceName { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public int ServiceDurationMinutes { get; set; }

    public string ProviderName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }

    public string BusinessName { get; set; } = string.Empty;
    public Guid BusinessId { get; set; }
    public string BusinessAddress { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }

    public bool HasReview { get; set; }
    public bool HasPayment { get; set; }

    public List<AppointmentStatusLogEntry> StatusHistory { get; set; } = new();
}

public class AppointmentStatusLogEntry
{
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
}
