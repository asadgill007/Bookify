using Bookify.Domain.Common;

namespace Bookify.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<Domain.Entities.User>
{
    Task<Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.User>> GetFilteredAsync(
        Domain.Enums.UserRole? roleFilter,
        bool? suspendedFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> GetFilteredCountAsync(
        Domain.Enums.UserRole? roleFilter,
        bool? suspendedFilter,
        CancellationToken cancellationToken = default);
    Task<Domain.Entities.User?> GetWithRoleAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IBusinessRepository : IRepository<Domain.Entities.Business>
{
    Task<Domain.Entities.Business?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Business>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(bool? verifiedFilter = null, bool? activeFilter = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Business>> GetFilteredAsync(
        bool? verifiedFilter,
        bool? activeFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> GetFilteredCountAsync(
        bool? verifiedFilter,
        bool? activeFilter,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Business>> GetByStatusAsync(
        Domain.Enums.VerificationStatus status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(
        Domain.Enums.VerificationStatus status,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Business>> SearchAsync(
        string? searchTerm,
        Guid? categoryId,
        double? latitude,
        double? longitude,
        double? radiusKm,
        double? minRating,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isVerified,
        string? sortBy,
        string sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> SearchCountAsync(
        string? searchTerm,
        Guid? categoryId,
        double? latitude,
        double? longitude,
        double? radiusKm,
        bool? isVerified = null,
        CancellationToken cancellationToken = default);
}

public interface IProviderRepository : IRepository<Domain.Entities.Provider>
{
    Task<IReadOnlyList<Domain.Entities.Provider>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task AddAvailabilityAsync(Domain.Entities.ProviderAvailability availability, CancellationToken cancellationToken = default);
    Task AddAvailabilityOverrideAsync(Domain.Entities.ProviderAvailabilityOverride overrideEntry, CancellationToken cancellationToken = default);
}

public interface IServiceRepository : IRepository<Domain.Entities.Service>
{
    Task<IReadOnlyList<Domain.Entities.Service>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
}

public interface IBusinessHoursRepository : IRepository<Domain.Entities.BusinessHours>
{
    Task<IReadOnlyList<Domain.Entities.BusinessHours>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.BusinessHours>> GetByBusinessIdsAsync(IReadOnlyList<Guid> businessIds, CancellationToken cancellationToken = default);
    Task ReplaceForBusinessAsync(Guid businessId, IReadOnlyList<Domain.Entities.BusinessHours> hours, CancellationToken cancellationToken = default);
}

public interface IAppointmentRepository : IRepository<Domain.Entities.Appointment>
{
    Task<Domain.Entities.Appointment?> GetByBookingReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Appointment>> GetUserAppointmentsAsync(
        Guid userId,
        bool isCustomer,
        Domain.Enums.AppointmentStatus? statusFilter,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> HasConflictAsync(
        Guid providerId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DateTime>> GetBookedSlotsAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Appointment>> GetByBusinessIdAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Appointment>> GetByBusinessIdDateRangeAsync(
        Guid businessId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
    Task<Domain.Entities.Appointment?> GetWithCustomerAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Appointment>> GetByStatusDateRangeAsync(
        Domain.Enums.AppointmentStatus status,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(Domain.Enums.AppointmentStatus? statusFilter = null, CancellationToken cancellationToken = default);
    Task<decimal> GetCompletedRevenueAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Appointment>> GetFutureByRecurringBookingAsync(
        Guid recurringBookingId,
        CancellationToken cancellationToken = default);
}

public interface IReviewRepository : IRepository<Domain.Entities.Review>
{
    Task<int> GetCountAsync(bool? publishedFilter = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Review>> GetFilteredAsync(
        bool? publishedFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Review>> GetByBusinessIdAsync(
        Guid businessId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> GetBusinessReviewCountAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Review>> GetByProviderIdAsync(
        Guid providerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> GetProviderReviewCountAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<bool> HasReviewForAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<(double AverageRating, int TotalReviews)> GetBusinessRatingAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<ReviewStatisticsResult> GetStatisticsAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopRatedProviderResult>> GetTopRatedProvidersAsync(int count, CancellationToken cancellationToken = default);
    Task<bool> HasCustomerVotedAsync(Guid reviewId, Guid customerId, CancellationToken cancellationToken = default);
    Task AddVoteAsync(Domain.Entities.ReviewVote vote, CancellationToken cancellationToken = default);
    Task AddReportAsync(Domain.Entities.ReviewReport report, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.ReviewReport>> GetReportsAsync(Domain.Entities.ReportStatus? statusFilter, int page, int pageSize, CancellationToken cancellationToken = default);
}

public class TopRatedProviderResult
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public class ReviewStatisticsResult
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
    public int TotalWithReplies { get; set; }
    public int TotalHidden { get; set; }
}

public interface IPaymentRepository : IRepository<Domain.Entities.Payment>
{
    Task<Domain.Entities.Payment?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Payment>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCustomerPaymentCountAsync(Guid customerId, CancellationToken cancellationToken = default);
}

public interface INotificationRepository : IRepository<Domain.Entities.Notification>
{
    Task<IReadOnlyList<Domain.Entities.Notification>> GetUserNotificationsAsync(
        Guid userId,
        bool? unreadOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IUserPreferenceRepository : IRepository<Domain.Entities.UserPreference>
{
    Task<Domain.Entities.UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ICategoryRepository : IRepository<Domain.Entities.Category>
{
    Task<Domain.Entities.Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Category>> GetActiveWithSubCategoriesAsync(CancellationToken cancellationToken = default);
}

public interface IRefreshTokenRepository : IRepository<Domain.Entities.RefreshToken>
{
    Task<Domain.Entities.RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.RefreshToken>> GetExpiredAsync(CancellationToken cancellationToken = default);
}
