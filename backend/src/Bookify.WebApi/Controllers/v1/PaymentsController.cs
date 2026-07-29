using Bookify.Application.Commands.Payments;
using Bookify.Application.Queries.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class PaymentsController : ApiController
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("initialize")]
    public async Task<IActionResult> Initialize([FromBody] InitializePaymentRequest request, CancellationToken cancellationToken)
    {
        var command = new InitializePaymentCommand
        {
            AppointmentId = request.AppointmentId,
            CustomerId = GetUserId(),
            PaymentMethod = request.PaymentMethod,
            ReturnUrl = request.ReturnUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? ApiOk(result.Data) : HandleResult(result);
    }

    [HttpPost("{transactionId}/confirm")]
    public async Task<IActionResult> Confirm(string transactionId, CancellationToken cancellationToken)
    {
        var command = new ConfirmPaymentCommand { TransactionId = transactionId };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPaymentQuery { PaymentId = id, UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPaymentHistoryQuery
        {
            UserId = GetUserId(),
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}

public class InitializePaymentRequest
{
    public Guid AppointmentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
}
