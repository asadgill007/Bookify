namespace Bookify.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IBusinessRepository Businesses { get; }
    IProviderRepository Providers { get; }
    IServiceRepository Services { get; }
    IBusinessHoursRepository BusinessHours { get; }
    IAppointmentRepository Appointments { get; }
    IReviewRepository Reviews { get; }
    IPaymentRepository Payments { get; }
    INotificationRepository Notifications { get; }
    IUserPreferenceRepository UserPreferences { get; }
    ICategoryRepository Categories { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IRecurringBookingRepository RecurringBookings { get; }
    IWaitlistRepository Waitlist { get; }
    IDocumentRepository Documents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
