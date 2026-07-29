namespace Bookify.Application.Interfaces;

/// <summary>
/// Fine-grained permission checking service.
/// Provides resource-level authorization beyond simple role checks.
/// </summary>
public interface IPermissionService
{
    Task<bool> CanManageBusinessAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default);
    Task<bool> CanManageProviderAsync(Guid userId, Guid providerId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessAppointmentAsync(Guid userId, Guid appointmentId, CancellationToken cancellationToken = default);
    Task<bool> CanManageReviewAsync(Guid userId, Guid reviewId, CancellationToken cancellationToken = default);
    Task<bool> IsBusinessOwnerAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default);
    Task<bool> IsProviderForBusinessAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default);
}
