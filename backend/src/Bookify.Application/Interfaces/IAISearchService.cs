using Bookify.Application.Common;

namespace Bookify.Application.Interfaces;

public class AIInterpretationResult
{
    public string Intent { get; set; } = string.Empty;
    public Dictionary<string, string> ExtractedFilters { get; set; } = new();
    public string SuggestedQuery { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}

public class AISearchRequest
{
    public string Query { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public interface IAISearchService
{
    Task<Result<AIInterpretationResult>> InterpretQueryAsync(AISearchRequest request, CancellationToken cancellationToken = default);
}
