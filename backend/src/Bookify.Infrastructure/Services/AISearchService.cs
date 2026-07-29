using Bookify.Application.Common;
using Bookify.Application.Interfaces;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Mock AI search service. Replace with actual AI integration (OpenAI, etc.) later.
/// Parses simple keywords from the query to extract intent and filters.
/// </summary>
public class AISearchService : IAISearchService
{
    public Task<Result<AIInterpretationResult>> InterpretQueryAsync(
        AISearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = new AIInterpretationResult
        {
            Intent = DetermineIntent(request.Query),
            ExtractedFilters = ExtractFilters(request.Query),
            SuggestedQuery = request.Query
        };

        return Task.FromResult(Result<AIInterpretationResult>.Success(result));
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

        return filters;
    }
}
