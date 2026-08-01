namespace Bookify.Application.Interfaces;

/// <summary>
/// A single exchange rate entry. Rates are expressed per 1 unit of the base currency.
/// </summary>
public sealed class CurrencyRate
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public decimal Rate { get; init; }
}

public sealed class CurrencyRatesResult
{
    public string BaseCurrency { get; init; } = "USD";
    public DateTime FetchedAt { get; init; }
    public IReadOnlyList<CurrencyRate> Rates { get; init; } = Array.Empty<CurrencyRate>();
}

/// <summary>
/// Converts amounts between currencies. Uses a static/cached exchange-rate
/// table when no live rates API key is configured; plug in a live provider
/// (e.g. Open Exchange Rates, Frankfurter) later without changing callers.
/// </summary>
public interface ICurrencyConversionService
{
    Task<CurrencyRatesResult> GetRatesAsync(CancellationToken cancellationToken = default);

    /// <summary>Converts amount from source currency to target currency (both 3-letter ISO codes).</summary>
    Task<decimal> ConvertAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default);

    string GetSymbol(string currencyCode);
}
