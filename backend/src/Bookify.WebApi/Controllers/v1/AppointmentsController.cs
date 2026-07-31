using Bookify.Application.Commands.Appointments;
using Bookify.Application.Common;
using Bookify.Application.DTOs.Appointments;
using Bookify.Application.Interfaces;
using Bookify.Application.Queries.Appointments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class AppointmentsController : ApiController
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new appointment.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAppointmentCommand
        {
            CustomerId = GetUserId(),
            ProviderId = request.ProviderId,
            ServiceId = request.ServiceId,
            BusinessId = request.BusinessId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CustomerNotes = request.CustomerNotes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? ApiCreated(result.Data!, "Appointment created successfully.")
            : HandleResult(result);
    }

    /// <summary>
    /// Get the current user's appointments (as customer or provider).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] string? role = "customer",
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAppointmentsQuery
        {
            UserId = GetUserId(),
            Role = role ?? "customer",
            Status = status,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get appointment details by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAppointmentByIdQuery
        {
            AppointmentId = id,
            UserId = GetUserId()
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel an appointment.
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest request, CancellationToken cancellationToken)
    {
        var command = new CancelAppointmentCommand
        {
            AppointmentId = id,
            UserId = GetUserId(),
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Confirm a pending appointment (Provider/BusinessOwner/Admin only).
    /// </summary>
    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "Provider,BusinessOwner,Admin")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var command = new ConfirmAppointmentCommand
        {
            AppointmentId = id,
            UserId = GetUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark a confirmed appointment as in progress (Provider/BusinessOwner/Admin only).
    /// This is required before an appointment can be completed.
    /// </summary>
    [HttpPut("{id}/start")]
    [Authorize(Roles = "Provider,BusinessOwner,Admin")]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        var command = new StartAppointmentCommand
        {
            AppointmentId = id,
            UserId = GetUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark appointment as completed (Provider/BusinessOwner/Admin only).
    /// </summary>
    [HttpPut("{id}/complete")]
    [Authorize(Roles = "Provider,BusinessOwner,Admin")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var command = new CompleteAppointmentCommand
        {
            AppointmentId = id,
            UserId = GetUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reschedule an appointment to a new time.
    /// </summary>
    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        var command = new RescheduleAppointmentCommand
        {
            AppointmentId = id,
            UserId = GetUserId(),
            NewStartTime = request.NewStartTime,
            NewEndTime = request.NewEndTime
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
