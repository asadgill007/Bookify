using Bookify.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class CurrenciesController : ApiController
{
    private readonly ICurrencyConversionService _currencyService;

    public CurrenciesController(ICurrencyConversionService currencyService)
    {
        _currencyService = currencyService;
    }

    /// <summary>List supported currencies with exchange rates (base USD).</summary>
    [HttpGet]
    public async Task<IActionResult> GetCurrencies(CancellationToken cancellationToken)
    {
        var rates = await _currencyService.GetRatesAsync(cancellationToken);
        return ApiOk(rates);
    }
}
