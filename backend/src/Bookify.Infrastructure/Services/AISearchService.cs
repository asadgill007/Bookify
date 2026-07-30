using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// AI Search service implementation with test mode support.
/// In test mode, uses simple keyword matching.
/// In production, integrates with OpenAI, Claude, or other AI providers.
/// </summary>
public class AISearchService : IAISearchService
{
    private readonly ILogger<AISearchService> _logger;
    private readonly AISearchSettings _settings;

    public AISearchService(
        ILogger<AISearchService> logger,
        IOptions<AISearchSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public Task<Result<AIInterpretationResult>> InterpretQueryAsync(
        AISearchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_settings.UseTestMode)
            {
                _logger.LogInformation("[AI SEARCH TEST MODE] Interpreting query: {Query}", request.Query);
                
                var result = new AIInterpretationResult
                {
                    Intent = DetermineIntent(request.Query),
                    ExtractedFilters = ExtractFilters(request.Query),
                    SuggestedQuery = request.Query,
                    Confidence = 0.85m
                };

                return Task.FromResult(Result<AIInterpretationResult>.Success(result));
            }

            // Production AI implementation
            // TODO: Integrate with OpenAI, Claude, or other AI provider
            _logger.LogWarning("[AI SEARCH PRODUCTION] AI provider not configured. Using fallback.");
            
            var fallbackResult = new AIInterpretationResult
            {
                Intent = "General Search",
                ExtractedFilters = new Dictionary<string, string>(),
                SuggestedQuery = request.Query,
                Confidence = 0.5m
            };

            return Task.FromResult(Result<AIInterpretationResult>.Success(fallbackResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to interpret search query: {Query}", request.Query);
            return Task.FromResult(Result<AIInterpretationResult>.Failure($"AI search failed: {ex.Message}"));
        }
    }

    private static string DetermineIntent(string query)
    {
        var lower = query.ToLowerInvariant();

        if (lower.Contains("dentist") || lower.Contains("dental") || lower.Contains("tooth"))
            return "Find Dentist";
        if (lower.Contains("salon") || lower.Contains("hair") || lower.Contains("barber"))
            return "Find Salon";
        if (lower.Contains("spa") || lower.Contains("massage") || lower.Contains("wellness"))
            return "Find Spa";
        if (lower.Contains("gym") || lower.Contains("fitness") || lower.Contains("trainer"))
            return "Find Fitness";
        if (lower.Contains("doctor") || lower.Contains("clinic") || lower.Contains("medical"))
            return "Find Medical";
        if (lower.Contains("restaurant") || lower.Contains("dining") || lower.Contains("food"))
            return "Find Dining";
        if (lower.Contains("book") || lower.Contains("appointment") || lower.Contains("schedule"))
            return "Book Appointment";
        if (lower.Contains("cancel") || lower.Contains("reschedule"))
            return "Modify Appointment";

        return "General Search";
    }

    private static Dictionary<string, string> ExtractFilters(string query)
    {
        var filters = new Dictionary<string, string>();
        var lower = query.ToLowerInvariant();

        if (lower.Contains("near me") || lower.Contains("nearby"))
            filters["location"] = "near me";

        if (lower.Contains("open") && (lower.Contains("sunday") || lower.Contains("saturday") || lower.Contains("weekend")))
            filters["openOn"] = "Weekend";

        if (lower.Contains("cheap") || lower.Contains("affordable") || lower.Contains("budget"))
            filters["priceLevel"] = "Low";

        if (lower.Contains("premium") || lower.Contains("luxury") || lower.Contains("best"))
            filters["priceLevel"] = "Premium";

        if (lower.Contains("today"))
            filters["date"] = "today";
        
        if (lower.Contains("tomorrow"))
            filters["date"] = "tomorrow";

        if (lower.Contains("this weekend") || lower.Contains("weekend"))
            filters["date"] = "weekend";

        return filters;
    }
}

public class AISearchSettings
{
    public bool UseTestMode { get; set; } = true;
    
    // OpenAI Settings
    public string OpenAIProvider { get; set; } = "OpenAI";
    public string OpenAIKey { get; set; } = string.Empty;
    public string OpenAIModel { get; set; } = "gpt-3.5-turbo";
    
    // Alternative: Claude Settings
    public string ClaudeKey { get; set; } = string.Empty;
    public string ClaudeModel { get; set; } = "claude-3-sonnet-20240229";
    
    // Alternative: Google Gemini Settings
    public string GeminiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "gemini-pro";
}
