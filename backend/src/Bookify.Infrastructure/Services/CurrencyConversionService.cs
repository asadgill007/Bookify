using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Currency conversion with a static/cached exchange-rate table (base USD).
/// No live API key required — plug in a real rates provider (Open Exchange
/// Rates, Frankfurter, exchangerate-api.com) by updating the seed table at
/// startup. The table below is a snapshot good enough for demo purposes.
/// </summary>
public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly ILogger<CurrencyConversionService> _logger;

    // Static snapshot rates per 1 USD (approx. mid-2026). Extend with more
    // currencies or replace with a live fetch when a rates API key is added.
    private static readonly Dictionary<string, (string Name, string Symbol, decimal Rate)> Table = new()
    {
        ["USD"] = ("US Dollar", "$", 1.00m),
        ["EUR"] = ("Euro", "€", 0.92m),
        ["GBP"] = ("British Pound", "£", 0.78m),
        ["PKR"] = ("Pakistani Rupee", "₨", 278.5m),
        ["AED"] = ("UAE Dirham", "د.إ", 3.67m),
        ["INR"] = ("Indian Rupee", "₹", 83.4m),
        ["CAD"] = ("Canadian Dollar", "C$", 1.37m),
        ["AUD"] = ("Australian Dollar", "A$", 1.51m),
        ["SAR"] = ("Saudi Riyal", "﷼", 3.75m),
        ["TRY"] = ("Turkish Lira", "₺", 32.9m),
        ["CNY"] = ("Chinese Yuan", "¥", 7.25m),
        ["JPY"] = ("Japanese Yen", "¥", 156.5m),
        ["BHD"] = ("Bahraini Dinar", ".د.ب", 0.376m),
        ["OMR"] = ("Omani Rial", "ر.ع.", 0.385m),
        ["QAR"] = ("Qatari Riyal", "ر.ق", 3.64m),
    };

    public CurrencyConversionService(ILogger<CurrencyConversionService> logger)
    {
        _logger = logger;
    }

    public Task<CurrencyRatesResult> GetRatesAsync(CancellationToken cancellationToken = default)
    {
        var rates = Table
            .OrderBy(kv => kv.Key)
            .Select(kv => new CurrencyRate
            {
                Code = kv.Key,
                Name = kv.Value.Name,
                Symbol = kv.Value.Symbol,
                Rate = kv.Value.Rate
            })
            .ToList();

        return Task.FromResult(new CurrencyRatesResult
        {
            BaseCurrency = "USD",
            FetchedAt = DateTime.UtcNow,
            Rates = rates
        });
    }

    public Task<decimal> ConvertAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default)
    {
        var from = fromCurrency.Trim().ToUpperInvariant();
        var to = toCurrency.Trim().ToUpperInvariant();

        if (from == to)
            return Task.FromResult(amount);

        // Convert via USD as the pivot to support arbitrary pairs.
        var fromUsd = Table.TryGetValue(from, out var f) ? amount / f.Rate : 0m;
        var converted = Table.TryGetValue(to, out var t) ? fromUsd * t.Rate : fromUsd;

        if (converted == 0m)
            _logger.LogWarning("Currency conversion for unknown pair {From} → {To}", from, to);

        return Task.FromResult(Math.Round(converted, 2));
    }

    public string GetSymbol(string currencyCode)
    {
        var code = currencyCode.Trim().ToUpperInvariant();
        return Table.TryGetValue(code, out var entry) ? entry.Symbol : code;
    }
}
