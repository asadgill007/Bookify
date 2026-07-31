using Bookify.Application.Commands.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/businesses/{businessId}/services")]
public class ServicesController : ApiController
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Add a new service (with name, description, duration, and price) to a business.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Create(Guid businessId, [FromBody] CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateServiceCommand
        {
            BusinessId = businessId,
            UserId = GetUserId(),
            Name = request.Name,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            PriceAmount = request.PriceAmount,
            Currency = request.Currency,
            Category = request.Category,
            DisplayOrder = request.DisplayOrder
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return HandleResult(result);

        return ApiCreated(new { ServiceId = result.Data }, "Service created successfully.");
    }

    /// <summary>
    /// Update an existing service.
    /// </summary>
    [HttpPut("{serviceId}")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Update(Guid businessId, Guid serviceId, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCommand
        {
            ServiceId = serviceId,
            UserId = GetUserId(),
            Name = request.Name,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            PriceAmount = request.PriceAmount,
            Currency = request.Currency,
            Category = request.Category,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete (soft-delete) a service.
    /// </summary>
    [HttpDelete("{serviceId}")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Delete(Guid businessId, Guid serviceId, CancellationToken cancellationToken)
    {
        var command = new DeleteServiceCommand
        {
            ServiceId = serviceId,
            UserId = GetUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public class CreateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
