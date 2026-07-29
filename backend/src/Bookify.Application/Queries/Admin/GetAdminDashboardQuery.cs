using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using MediatR;

namespace Bookify.Application.Queries.Admin;

public sealed record GetAdminDashboardQuery : IRequest<Result<AdminDashboardResult>>
{
    public Guid AdminUserId { get; init; }
}

public sealed class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetAdminDashboardQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<AdminDashboardResult>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var result = await _cache.GetOrCreateAsync(CacheKeys.AdminDashboard, async () =>
        {
            // Count queries execute entirely at the database level
            var totalUsers = await _unitOfWork.Users.GetFilteredCountAsync(null, null, cancellationToken);
            var totalCustomers = await _unitOfWork.Users.GetFilteredCountAsync(UserRole.Customer, null, cancellationToken);
            var totalProviders = await _unitOfWork.Users.GetFilteredCountAsync(UserRole.Provider, null, cancellationToken);
            var totalBusinessOwners = await _unitOfWork.Users.GetFilteredCountAsync(UserRole.BusinessOwner, null, cancellationToken);
            var totalAdmins = await _unitOfWork.Users.GetFilteredCountAsync(UserRole.Admin, null, cancellationToken);
            var suspendedUsers = await _unitOfWork.Users.GetFilteredCountAsync(null, true, cancellationToken);

            var totalBusinesses = await _unitOfWork.Businesses.GetCountAsync(null, null, cancellationToken);
            var verifiedBusinesses = await _unitOfWork.Businesses.GetCountAsync(true, null, cancellationToken);
            var activeBusinesses = await _unitOfWork.Businesses.GetCountAsync(null, true, cancellationToken);
            var inactiveBusinesses = await _unitOfWork.Businesses.GetCountAsync(null, false, cancellationToken);

            var totalAppointments = await _unitOfWork.Appointments.GetCountAsync(null, cancellationToken);
            var pendingAppts = await _unitOfWork.Appointments.GetCountAsync(AppointmentStatus.Pending, cancellationToken);
            var confirmedAppts = await _unitOfWork.Appointments.GetCountAsync(AppointmentStatus.Confirmed, cancellationToken);
            var completedAppts = await _unitOfWork.Appointments.GetCountAsync(AppointmentStatus.Completed, cancellationToken);
            var cancelledAppts = await _unitOfWork.Appointments.GetCountAsync(AppointmentStatus.Cancelled, cancellationToken);

            var totalReviews = await _unitOfWork.Reviews.GetCountAsync(null, cancellationToken);
            var publishedReviews = await _unitOfWork.Reviews.GetCountAsync(true, cancellationToken);
            var unpublishedReviews = await _unitOfWork.Reviews.GetCountAsync(false, cancellationToken);

            var totalRevenue = await _unitOfWork.Appointments.GetCompletedRevenueAsync(cancellationToken);

            return new AdminDashboardResult
            {
                TotalUsers = totalUsers,
                TotalCustomers = totalCustomers,
                TotalProviders = totalProviders,
                TotalBusinessOwners = totalBusinessOwners,
                TotalAdmins = totalAdmins,
                SuspendedUsers = suspendedUsers,

                TotalBusinesses = totalBusinesses,
                VerifiedBusinesses = verifiedBusinesses,
                PendingVerification = totalBusinesses - (verifiedBusinesses + inactiveBusinesses),
                InactiveBusinesses = inactiveBusinesses,

                TotalAppointments = totalAppointments,
                PendingAppointments = pendingAppts,
                ConfirmedAppointments = confirmedAppts,
                CompletedAppointments = completedAppts,
                CancelledAppointments = cancelledAppts,

                TotalReviews = totalReviews,
                PublishedReviews = publishedReviews,
                UnpublishedReviews = unpublishedReviews,

                TotalRevenue = totalRevenue
            };
        }, CacheEntryOptions.Statistics, cancellationToken);

        return Result<AdminDashboardResult>.Success(result);
    }
}

public class AdminDashboardResult
{
    // Users
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProviders { get; set; }
    public int TotalBusinessOwners { get; set; }
    public int TotalAdmins { get; set; }
    public int SuspendedUsers { get; set; }

    // Businesses
    public int TotalBusinesses { get; set; }
    public int VerifiedBusinesses { get; set; }
    public int PendingVerification { get; set; }
    public int InactiveBusinesses { get; set; }

    // Appointments
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }

    // Reviews
    public int TotalReviews { get; set; }
    public int PublishedReviews { get; set; }
    public int UnpublishedReviews { get; set; }

    // Revenue
    public decimal TotalRevenue { get; set; }
}
