using Bookify.Application.Common;
using Bookify.Application.DTOs.Appointments;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using MediatR;

namespace Bookify.Application.Queries.Dashboard;

public sealed class GetCustomerDashboardQuery : IRequest<Result<CustomerDashboardResult>>
{
    public Guid UserId { get; init; }
}

public sealed class GetCustomerDashboardQueryHandler : IRequestHandler<GetCustomerDashboardQuery, Result<CustomerDashboardResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerDashboardResult>> Handle(GetCustomerDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Get upcoming appointments (future confirmed/pending)
        var upcoming = await _unitOfWork.Appointments.GetUserAppointmentsAsync(
            request.UserId, true, null, now, null, 1, 50, cancellationToken);

        var upcomingAppointments = upcoming
            .Where(a => a.Status == AppointmentStatus.Pending
                     || a.Status == AppointmentStatus.Confirmed)
            .Select(a => new AppointmentDto
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
                BusinessName = a.Business?.Name ?? "",
                BusinessAddress = a.Business != null
                    ? $"{a.Business.AddressLine1}, {a.Business.City}"
                    : "",
                CreatedAt = a.CreatedAt
            })
            .OrderBy(a => a.StartTime)
            .ToList();

        // Get past appointments (completed/cancelled)
        var past = await _unitOfWork.Appointments.GetUserAppointmentsAsync(
            request.UserId, true, null, null, now, 1, 1000, cancellationToken);

        var completedCount = past.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelledCount = past.Count(a => a.Status == AppointmentStatus.Cancelled);
        var totalSpent = past
            .Where(a => a.Status == AppointmentStatus.Completed)
            .Sum(a => a.TotalAmount);

        var unreadNotifications = await _unitOfWork.Notifications.GetUnreadCountAsync(request.UserId, cancellationToken);

        var result = new CustomerDashboardResult
        {
            UpcomingAppointments = upcomingAppointments.Count,
            UpcomingAppointmentList = upcomingAppointments,
            PastAppointments = completedCount,
            CancelledAppointments = cancelledCount,
            TotalSpent = totalSpent,
            Currency = "USD",
            UnreadNotifications = unreadNotifications
        };

        return Result<CustomerDashboardResult>.Success(result);
    }
}

public class CustomerDashboardResult
{
    public int UpcomingAppointments { get; set; }
    public List<AppointmentDto> UpcomingAppointmentList { get; set; } = new();
    public int PastAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public decimal TotalSpent { get; set; }
    public string Currency { get; set; } = "USD";
    public int UnreadNotifications { get; set; }
}
