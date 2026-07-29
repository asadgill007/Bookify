using Bookify.Application.Commands.Settings;
using Bookify.Application.DTOs.Settings;
using Bookify.Application.Queries.Settings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class SettingsController : ApiController
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var query = new GetUserPreferencesQuery { UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserPreferencesCommand
        {
            UserId = GetUserId(),
            Language = request.Language,
            Currency = request.Currency,
            Interests = request.Interests,
            IsDarkMode = request.IsDarkMode,
            IsAmoledMode = request.IsAmoledMode,
            NotificationsEnabled = request.NotificationsEnabled,
            MarketingEmails = request.MarketingEmails
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
