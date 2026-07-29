using Bookify.Application.Commands.Appointments;
using Bookify.Application.Queries.Appointments;
using Bookify.Application.Queries.Providers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class ProvidersController : ApiController
{
    private readonly IMediator _mediator;

    public ProvidersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get available time slots for a provider on a specific date.
    /// </summary>
    [HttpGet("{providerId}/slots")]
    public async Task<IActionResult> GetAvailableSlots(
        Guid providerId,
        [FromQuery] Guid? serviceId = null,
        [FromQuery] DateOnly? date = null,
        [FromQuery] int bufferMinutes = 0,
        CancellationToken cancellationToken = default)
    {
        if (date == null)
            return ApiBadRequest("Date is required.");

        var query = new GetAvailableSlotsQuery
        {
            ProviderId = providerId,
            ServiceId = serviceId,
            BusinessId = Guid.Empty, // Will be resolved from provider
            Date = date.Value,
            BufferMinutes = bufferMinutes
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Set weekly availability for a provider.
    /// </summary>
    [HttpPut("{providerId}/availability")]
    [Authorize(Roles = "Provider,BusinessOwner,Admin")]
    public async Task<IActionResult> SetAvailability(
        Guid providerId,
        [FromBody] SetAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetWeeklyAvailabilityCommand
        {
            ProviderId = providerId,
            UserId = GetUserId(),
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDurationMinutes = request.SlotDurationMinutes,
            IsAvailable = request.IsAvailable
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Add an availability override (holiday, leave, extra hours).
    /// </summary>
    [HttpPost("{providerId}/availability/overrides")]
    [Authorize(Roles = "Provider,BusinessOwner,Admin")]
    public async Task<IActionResult> AddOverride(
        Guid providerId,
        [FromBody] AddOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddAvailabilityOverrideCommand
        {
            ProviderId = providerId,
            UserId = GetUserId(),
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = request.IsAvailable,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get provider details by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProviderByIdQuery { ProviderId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}

public class SetAvailabilityRequest
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 60;
    public bool IsAvailable { get; set; } = true;
}

public class AddOverrideRequest
{
    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Reason { get; set; }
}
