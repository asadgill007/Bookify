using Bookify.Domain.Common;
using Bookify.Domain.Enums;
using Bookify.Domain.ValueObjects;

namespace Bookify.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsBiometricEnabled { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string PreferredLanguage { get; private set; }
    public string PreferredCurrency { get; private set; }
    public bool IsSuspended { get; private set; }
    public DateTime? SuspendedAt { get; private set; }
    public Guid? SuspendedBy { get; private set; }
    public string? SuspensionReason { get; private set; }

    public UserPreference? Preference { get; private set; }
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();

    private User() { }

    public User(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role = UserRole.Customer,
        string? phoneNumber = null)
    {
        SetName(firstName, lastName);
        Email = email.ToLowerInvariant().Trim();
        PasswordHash = passwordHash;
        Role = role;
        PhoneNumber = phoneNumber;
        PreferredLanguage = "en";
        PreferredCurrency = "USD";
    }

    public void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
        if (firstName.Length > 100)
            throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));
        if (lastName.Length > 100)
            throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Touch();
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        SetName(firstName, lastName);
        PhoneNumber = phoneNumber?.Trim();
        Touch();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        Touch();
    }

    public void SetAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        Touch();
    }

    public void ToggleBiometric(bool enabled)
    {
        IsBiometricEnabled = enabled;
        Touch();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        Touch();
    }

    public void UpdatePreferences(string language, string currency)
    {
        PreferredLanguage = language;
        PreferredCurrency = currency;
        Touch();
    }

    public void SetRole(UserRole newRole)
    {
        Role = newRole;
        Touch();
    }

    public void Suspend(string? reason = null)
    {
        IsSuspended = true;
        SuspendedAt = DateTime.UtcNow;
        SuspensionReason = reason?.Trim();
        Touch();
    }

    public void Unsuspend()
    {
        IsSuspended = false;
        SuspendedAt = null;
        SuspendedBy = null;
        SuspensionReason = null;
        Touch();
    }

    public void SetSuspendedBy(Guid suspendedBy)
    {
        SuspendedBy = suspendedBy;
    }

    public string FullName => $"{FirstName} {LastName}";
}
