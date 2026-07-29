using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public sealed class UserPreference : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Language { get; private set; }
    public string Currency { get; private set; }
    public string? Interests { get; private set; }
    public bool IsDarkMode { get; private set; }
    public bool IsAmoledMode { get; private set; }
    public bool NotificationsEnabled { get; private set; }
    public bool MarketingEmails { get; private set; }

    public User User { get; private set; } = null!;

    private UserPreference() { }

    public UserPreference(Guid userId)
    {
        UserId = userId;
        Language = "en";
        Currency = "USD";
        NotificationsEnabled = true;
    }

    public void Update(
        string language,
        string currency,
        string? interests,
        bool isDarkMode,
        bool isAmoledMode,
        bool notificationsEnabled,
        bool marketingEmails)
    {
        Language = language;
        Currency = currency;
        Interests = interests;
        IsDarkMode = isDarkMode;
        IsAmoledMode = isAmoledMode;
        NotificationsEnabled = notificationsEnabled;
        MarketingEmails = marketingEmails;
        Touch();
    }
}
