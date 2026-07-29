using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Stub permission service. Replace with real resource-ownership checks.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IUnitOfWork unitOfWork, ILogger<PermissionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> CanManageBusinessAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId, cancellationToken);
        if (business == null) return false;
        return business.OwnerId == userId;
    }

    public async Task<bool> CanManageProviderAsync(Guid userId, Guid providerId, CancellationToken cancellationToken = default)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(providerId, cancellationToken);
        if (provider == null) return false;

        var business = await _unitOfWork.Businesses.GetByIdAsync(provider.BusinessId, cancellationToken);
        return business?.OwnerId == userId || provider.UserId == userId;
    }

    public async Task<bool> CanAccessAppointmentAsync(Guid userId, Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null) return false;
        return appointment.CustomerId == userId || appointment.Provider?.UserId == userId;
    }

    public async Task<bool> CanManageReviewAsync(Guid userId, Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId, cancellationToken);
        if (review == null) return false;
        return review.CustomerId == userId;
    }

    public async Task<bool> IsBusinessOwnerAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId, cancellationToken);
        if (business == null) return false;
        return business.OwnerId == userId;
    }

    public async Task<bool> IsProviderForBusinessAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default)
    {
        var providers = await _unitOfWork.Providers.GetByBusinessIdAsync(businessId, cancellationToken);
        return providers.Any(p => p.UserId == userId);
    }
}
