using System.Security.Claims;
using Bookify.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bookify.WebApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Api")]
public abstract class ApiController : ControllerBase
{
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException();

        return Guid.Parse(userIdClaim);
    }

    protected string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    protected string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    protected IActionResult OkOrNotFound<T>(T? data)
    {
        return data is null ? NotFound() : Ok(data);
    }

    protected IActionResult ApiOk<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    protected IActionResult ApiOk<T>(T data, PaginationInfo pagination, string? message = null)
    {
        return Ok(ApiResponse<T>.Ok(data, pagination, message));
    }

    protected IActionResult ApiOk(string? message = null)
    {
        return Ok(ApiResponse.Ok(message));
    }

    protected IActionResult ApiCreated<T>(T data, string? message = null)
    {
        return Created(string.Empty, ApiResponse<T>.Ok(data, message ?? "Resource created successfully."));
    }

    protected IActionResult ApiBadRequest(string message, object? errors = null)
    {
        return BadRequest(ApiResponse.Fail(message, errors));
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return ApiOk(result.Data);

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(ApiResponse.Fail(result.Error!)),
            "INVALID_CREDENTIALS" => Unauthorized(ApiResponse.Fail(result.Error!)),
            "FORBIDDEN" => StatusCode(403, ApiResponse.Fail(result.Error!)),
            _ => ApiBadRequest(result.Error!)
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return ApiOk();

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(ApiResponse.Fail(result.Error!)),
            "INVALID_CREDENTIALS" => Unauthorized(ApiResponse.Fail(result.Error!)),
            "FORBIDDEN" => StatusCode(403, ApiResponse.Fail(result.Error!)),
            _ => ApiBadRequest(result.Error!)
        };
    }
}
