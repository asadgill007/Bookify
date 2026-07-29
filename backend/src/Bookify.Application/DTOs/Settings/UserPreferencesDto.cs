using FluentValidation;

namespace Bookify.Application.DTOs.Settings;

public class UserPreferencesDto
{
    public string Language { get; set; } = "en";
    public string Currency { get; set; } = "USD";
    public List<string>? Interests { get; set; }
    public bool IsDarkMode { get; set; }
    public bool IsAmoledMode { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool MarketingEmails { get; set; }
}

public class UpdatePreferencesRequest
{
    public string Language { get; set; } = "en";
    public string Currency { get; set; } = "USD";
    public List<string>? Interests { get; set; }
    public bool IsDarkMode { get; set; }
    public bool IsAmoledMode { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool MarketingEmails { get; set; }
}

public class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequest>
{
    public UpdatePreferencesRequestValidator()
    {
        RuleFor(x => x.Language).NotEmpty().Length(2, 10);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
