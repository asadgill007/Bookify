using Bookify.Application.Commands.RecurringBookings;
using Bookify.Application.Queries.RecurringBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class RecurringBookingsController : ApiController
{
    private readonly IMediator _mediator;

    public RecurringBookingsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a new recurring booking series.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringBookingRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRecurringBookingCommand
        {
            CustomerId = GetUserId(),
            ProviderId = request.ProviderId,
            ServiceId = request.ServiceId,
            BusinessId = request.BusinessId,
            RecurrenceType = request.RecurrenceType,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SeriesStartDate = request.SeriesStartDate,
            SeriesEndDate = request.SeriesEndDate,
            MaxOccurrences = request.MaxOccurrences,
            Interval = request.Interval,
            DayOfMonth = request.DayOfMonth,
            DaysOfWeek = request.DaysOfWeek ?? new(),
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? ApiCreated(new { }, "Recurring booking created.") : HandleResult(result);
    }

    /// <summary>Get all recurring bookings for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string role = "customer", CancellationToken cancellationToken = default)
    {
        var query = new GetRecurringBookingsQuery { UserId = GetUserId(), Role = role };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Cancel an entire recurring series.</summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelSeries(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelRecurringSeriesCommand { RecurringBookingId = id, UserId = GetUserId() };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Skip the next upcoming occurrence.</summary>
    [HttpPut("{id}/skip-next")]
    public async Task<IActionResult> SkipNext(Guid id, CancellationToken cancellationToken)
    {
        var command = new SkipRecurringOccurrenceCommand { RecurringBookingId = id, UserId = GetUserId() };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Update future occurrences of the series.</summary>
    [HttpPut("{id}/update-future")]
    public async Task<IActionResult> UpdateFuture(Guid id, [FromBody] UpdateFutureRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateRecurringScheduleCommand
        {
            RecurringBookingId = id,
            UserId = GetUserId(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SeriesEndDate = request.SeriesEndDate,
            MaxOccurrences = request.MaxOccurrences,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public class CreateRecurringBookingRequest
{
    public Guid ProviderId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid BusinessId { get; set; }
    public Domain.Entities.RecurrenceType RecurrenceType { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime SeriesStartDate { get; set; }
    public DateTime? SeriesEndDate { get; set; }
    public int? MaxOccurrences { get; set; }
    public int Interval { get; set; } = 1;
    public int? DayOfMonth { get; set; }
    public List<DayOfWeek>? DaysOfWeek { get; set; }
    public string? Notes { get; set; }
}

public class UpdateFutureRequest
{
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public DateTime? SeriesEndDate { get; set; }
    public int? MaxOccurrences { get; set; }
    public string? Notes { get; set; }
}
