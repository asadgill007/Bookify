namespace Bookify.Application.Interfaces;

/// <summary>
/// Handles automated waitlist promotion when appointments become available.
/// </summary>
public interface IWaitlistPromotionService
{
    /// <summary>Promote the next waiting customer when an appointment slot opens up.</summary>
    Task<PromotionResult> PromoteNextAsync(Guid providerId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken = default);

    /// <summary>Expire all entries past their expiration date.</summary>
    Task<int> ExpireOldEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Get the count of waiting customers for a given provider and date.</summary>
    Task<int> GetWaitCountAsync(Guid providerId, DateOnly date, CancellationToken cancellationToken = default);
}

public class PromotionResult
{
    public bool IsPromoted { get; set; }
    public Guid? EntryId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Message { get; set; }
}
