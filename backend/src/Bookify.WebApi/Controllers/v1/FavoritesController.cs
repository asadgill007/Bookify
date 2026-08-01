using Bookify.Application.Commands.Favorites;
using Bookify.Application.Queries.Favorites;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize(Roles = "Customer,Provider,BusinessOwner,Admin")]
public class FavoritesController : ApiController
{
    private readonly IMediator _mediator;

    public FavoritesController(IMediator mediator) => _mediator = mediator;

    /// <summary>List the current user's favorite businesses.</summary>
    [HttpGet]
    public async Task<IActionResult> GetFavorites(CancellationToken cancellationToken)
    {
        var query = new GetFavoritesQuery { UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Get just the ids of favorited businesses (for heart state).</summary>
    [HttpGet("ids")]
    public async Task<IActionResult> GetFavoriteIds(CancellationToken cancellationToken)
    {
        var query = new GetFavoriteIdsQuery { UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Add a business to favorites.</summary>
    [HttpPost("{businessId}")]
    public async Task<IActionResult> AddFavorite(Guid businessId, CancellationToken cancellationToken)
    {
        var command = new AddFavoriteCommand { UserId = GetUserId(), BusinessId = businessId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? ApiOk(message: "Added to favorites.")
            : HandleResult(result);
    }

    /// <summary>Remove a business from favorites.</summary>
    [HttpDelete("{businessId}")]
    public async Task<IActionResult> RemoveFavorite(Guid businessId, CancellationToken cancellationToken)
    {
        var command = new RemoveFavoriteCommand { UserId = GetUserId(), BusinessId = businessId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? ApiOk(message: "Removed from favorites.")
            : HandleResult(result);
    }
}
