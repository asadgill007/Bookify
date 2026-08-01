using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Evaluates the business completeness checklist and auto-promotes Pending
/// businesses to Approved the moment all required information is present.
/// </summary>
public class BusinessVerificationService : IBusinessVerificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BusinessVerificationService> _logger;

    public BusinessVerificationService(IUnitOfWork unitOfWork, ILogger<BusinessVerificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BusinessChecklistResult> EvaluateAndAutoVerifyAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var business = await _unitOfWork.Businesses.GetByIdWithDetailsAsync(businessId, cancellationToken);
        if (business == null)
        {
            return new BusinessChecklistResult { IsComplete = false };
        }

        var hasCategory = business.BusinessCategories.Any(bc => !bc.IsDeleted);
        var hasHours = business.BusinessHours.Any(h => !h.IsClosed);
        var hasServiceWithPricing = business.Services.Any(s => s.IsActive && s.PriceAmount >= 0);
        var hasImage = business.Images.Any(i => !i.IsDeleted) ||
                       !string.IsNullOrWhiteSpace(business.CoverImageUrl) ||
                       !string.IsNullOrWhiteSpace(business.LogoUrl);
        var hasContact = !string.IsNullOrWhiteSpace(business.PhoneNumber) ||
                         !string.IsNullOrWhiteSpace(business.Email);

        var items = new List<BusinessChecklistItem>
        {
            new() { Key = "name", Label = "Business name", IsComplete = !string.IsNullOrWhiteSpace(business.Name) },
            new() { Key = "description", Label = "Description", IsComplete = !string.IsNullOrWhiteSpace(business.Description) },
            new() { Key = "category", Label = "Category selected", IsComplete = hasCategory },
            new() { Key = "address", Label = "Address", IsComplete = !string.IsNullOrWhiteSpace(business.AddressLine1) },
            new() { Key = "contact", Label = "Contact phone or email", IsComplete = hasContact },
            new() { Key = "hours", Label = "Opening hours", IsComplete = hasHours },
            new() { Key = "service", Label = "At least one priced service", IsComplete = hasServiceWithPricing },
            new() { Key = "image", Label = "At least one cover/profile image", IsComplete = hasImage },
        };

        var isComplete = items.All(i => i.IsComplete);

        // Auto-promote Pending → Approved the moment the checklist passes.
        // Rejected businesses are NOT auto-promoted (admin override stands until
        // the owner explicitly resubmits); Approved businesses stay approved.
        if (isComplete && business.VerificationStatus == Domain.Enums.VerificationStatus.Pending)
        {
            business.Verify(adminUserId: null);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Business {BusinessId} ({Name}) auto-verified: checklist complete.",
                business.Id, business.Name);
        }

        return new BusinessChecklistResult
        {
            IsComplete = isComplete,
            VerificationStatus = business.VerificationStatus,
            Items = items
        };
    }
}
