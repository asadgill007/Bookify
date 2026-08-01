using Bookify.Domain.Enums;

namespace Bookify.Application.Interfaces;

/// <summary>
/// One item of the business "complete information" checklist.
/// </summary>
public sealed class BusinessChecklistItem
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool IsComplete { get; init; }
}

/// <summary>
/// Result of evaluating a business against the auto-verification checklist.
/// </summary>
public sealed class BusinessChecklistResult
{
    public bool IsComplete { get; init; }
    public VerificationStatus VerificationStatus { get; init; }
    public IReadOnlyList<BusinessChecklistItem> Items { get; init; } =
        Array.Empty<BusinessChecklistItem>();
}

/// <summary>
/// Evaluates the business completeness checklist and automatically promotes
/// Pending businesses to Approved the moment all required information is
/// present — no admin action needed. Admin manual verify/reject remain as
/// overrides.
/// </summary>
public interface IBusinessVerificationService
{
    /// <summary>
    /// Loads the business with all detail collections, evaluates the
    /// checklist, and if everything is complete and the business is still
    /// Pending, auto-verifies it. Returns the evaluated checklist result.
    /// </summary>
    Task<BusinessChecklistResult> EvaluateAndAutoVerifyAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);
}
