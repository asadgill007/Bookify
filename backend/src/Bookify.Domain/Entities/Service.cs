using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public sealed class Service : BaseEntity
{
    public Guid BusinessId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int DurationMinutes { get; private set; }
    public decimal PriceAmount { get; private set; }
    public string PriceCurrency { get; private set; }
    public string? Category { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    public Business Business { get; private set; } = null!;
    public ICollection<Appointment> Appointments { get; private set; } = new List<Appointment>();
    public ICollection<ProviderService> ProviderServices { get; private set; } = new List<ProviderService>();

    private Service() { }

    public Service(
        Guid businessId,
        string name,
        int durationMinutes,
        decimal priceAmount,
        string priceCurrency = "USD")
    {
        BusinessId = businessId;
        SetName(name);
        SetDuration(durationMinutes);
        SetPrice(priceAmount, priceCurrency);
        IsActive = true;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name cannot be empty.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Service name cannot exceed 200 characters.", nameof(name));

        Name = name.Trim();
        Touch();
    }

    public void SetDuration(int minutes)
    {
        if (minutes < 5 || minutes > 1440)
            throw new ArgumentException("Duration must be between 5 and 1440 minutes.", nameof(minutes));

        DurationMinutes = minutes;
        Touch();
    }

    public void SetPrice(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        PriceAmount = Math.Round(amount, 2);
        PriceCurrency = currency.ToUpperInvariant();
        Touch();
    }

    public void UpdateDetails(string? description, string? category, int displayOrder, bool isActive)
    {
        Description = description?.Trim();
        Category = category?.Trim();
        DisplayOrder = displayOrder;
        IsActive = isActive;
        Touch();
    }
}
