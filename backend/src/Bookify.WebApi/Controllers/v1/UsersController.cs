using Bookify.Application.Commands.Users;
using Bookify.Application.DTOs.Users;
using Bookify.Application.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var query = new GetUserProfileQuery { UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = GetUserId(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand
        {
            UserId = GetUserId(),
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        var command = new DeleteAccountCommand { UserId = GetUserId() };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("me/biometric")]
    public async Task<IActionResult> ToggleBiometric([FromBody] ToggleBiometricRequest request, CancellationToken cancellationToken)
    {
        var command = new ToggleBiometricCommand
        {
            UserId = GetUserId(),
            Enabled = request.Enabled
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public class ToggleBiometricRequest
{
    public bool Enabled { get; set; }
}
