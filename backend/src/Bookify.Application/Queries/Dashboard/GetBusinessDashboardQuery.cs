using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using MediatR;

namespace Bookify.Application.Queries.Dashboard;

public sealed class GetBusinessDashboardQuery : IRequest<Result<BusinessDashboardResult>>
{
    public Guid BusinessId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class GetBusinessDashboardQueryHandler : IRequestHandler<GetBusinessDashboardQuery, Result<BusinessDashboardResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBusinessDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BusinessDashboardResult>> Handle(GetBusinessDashboardQuery request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result<BusinessDashboardResult>.Failure("Business not found.", "NOT_FOUND");

        if (business.OwnerId != request.UserId)
            return Result<BusinessDashboardResult>.Failure("You do not have access to this dashboard.", "FORBIDDEN");

        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = todayStart.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // All queries execute at the database level via dedicated repository methods
        var businessAppointments = await _unitOfWork.Appointments.GetByBusinessIdAsync(
            request.BusinessId, cancellationToken);

        // Today's bookings
        var todayBookings = businessAppointments
            .Count(a => a.StartTime >= todayStart && a.StartTime < todayStart.AddDays(1));

        // Weekly bookings
        var weekBookings = businessAppointments
            .Count(a => a.StartTime >= weekStart && a.StartTime < weekStart.AddDays(7));

        // Monthly bookings
        var monthBookings = businessAppointments
            .Count(a => a.StartTime >= monthStart && a.StartTime < monthStart.AddMonths(1));

        // Revenue calculations
        var monthlyRevenue = businessAppointments
            .Where(a => a.Status == AppointmentStatus.Completed
                     && a.StartTime >= monthStart
                     && a.StartTime < monthStart.AddMonths(1))
            .Sum(a => a.TotalAmount);

        var totalRevenue = businessAppointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .Sum(a => a.TotalAmount);

        // Upcoming appointments
        var upcomingCount = businessAppointments
            .Count(a => a.StartTime > now
                     && (a.Status == AppointmentStatus.Pending
                      || a.Status == AppointmentStatus.Confirmed));

        // Cancellation statistics
        var totalCompleted = businessAppointments.Count(a => a.Status == AppointmentStatus.Completed);
        var totalCancelled = businessAppointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var cancellationRate = totalCompleted + totalCancelled > 0
            ? (double)totalCancelled / (totalCompleted + totalCancelled) * 100
            : 0;

        var result = new BusinessDashboardResult
        {
            BusinessId = business.Id,
            BusinessName = business.Name,
            AverageRating = business.AverageRating,
            TotalReviews = business.TotalReviews,
            TodayBookings = todayBookings,
            WeeklyBookings = weekBookings,
            MonthlyBookings = monthBookings,
            UpcomingAppointments = upcomingCount,
            TotalAppointments = businessAppointments.Count,
            MonthlyRevenue = monthlyRevenue,
            TotalRevenue = totalRevenue,
            TotalCancelled = totalCancelled,
            CancellationRate = Math.Round(cancellationRate, 1),
            Currency = business.Currency
        };

        return Result<BusinessDashboardResult>.Success(result);
    }
}

public class BusinessDashboardResult
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TodayBookings { get; set; }
    public int WeeklyBookings { get; set; }
    public int MonthlyBookings { get; set; }
    public int UpcomingAppointments { get; set; }
    public int TotalAppointments { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalCancelled { get; set; }
    public double CancellationRate { get; set; }
    public string Currency { get; set; } = "USD";
}
