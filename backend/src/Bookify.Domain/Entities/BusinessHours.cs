using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

/// <summary>
/// Weekly opening hours for a business, distinct from per-provider availability.
/// One row per day of the week.
/// </summary>
public sealed class BusinessHours : BaseEntity
{
    public Guid BusinessId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    public Business Business { get; private set; } = null!;

    private BusinessHours() { }

    public BusinessHours(
        Guid businessId,
        DayOfWeek dayOfWeek,
        TimeOnly openTime,
        TimeOnly closeTime,
        bool isClosed = false)
    {
        BusinessId = businessId;
        DayOfWeek = dayOfWeek;
        SetTimeRange(openTime, closeTime);
        IsClosed = isClosed;
    }

    public void SetTimeRange(TimeOnly openTime, TimeOnly closeTime)
    {
        if (openTime >= closeTime)
            throw new ArgumentException("Opening time must be before closing time.");

        OpenTime = openTime;
        CloseTime = closeTime;
        Touch();
    }

    public void SetClosed(bool isClosed)
    {
        IsClosed = isClosed;
        Touch();
    }
}
