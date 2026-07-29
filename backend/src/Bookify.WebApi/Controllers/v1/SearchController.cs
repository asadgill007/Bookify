using Bookify.Application.Queries.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class SearchController : ApiController
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("ai")]
    public async Task<IActionResult> AISearch(
        [FromQuery] string query,
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        CancellationToken cancellationToken = default)
    {
        var searchQuery = new SearchAIQuery
        {
            Query = query,
            Latitude = latitude,
            Longitude = longitude
        };

        var result = await _mediator.Send(searchQuery, cancellationToken);
        return HandleResult(result);
    }
}
