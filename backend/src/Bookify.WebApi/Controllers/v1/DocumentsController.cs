using Bookify.Application.Commands.Documents;
using Bookify.Application.Queries.Documents;
using Bookify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class DocumentsController : ApiController
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Upload a document to a business (optionally linked to an appointment or provider).</summary>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> Upload(
        [FromForm] Guid businessId,
        [FromForm] Guid? appointmentId,
        [FromForm] Guid? providerId,
        [FromForm] string documentType,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return ApiBadRequest("File is required.");

        if (!Enum.TryParse<DocumentType>(documentType, true, out var docType))
            return ApiBadRequest($"Invalid document type. Valid: {string.Join(", ", Enum.GetNames<DocumentType>())}");

        await using var stream = file.OpenReadStream();

        var command = new UploadDocumentCommand
        {
            BusinessId = businessId,
            UploadedByUserId = GetUserId(),
            AppointmentId = appointmentId,
            ProviderId = providerId,
            DocumentType = docType,
            FileName = file.FileName,
            Content = stream,
            ContentType = file.ContentType,
            IsAdmin = User.IsInRole("Admin")
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? ApiCreated(new { DocumentId = result.Data }, "Document uploaded successfully.")
            : HandleResult(result);
    }

    /// <summary>Download a document by its ID.</summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var query = new DownloadDocumentQuery { DocumentId = id, UserId = GetUserId() };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess || result.Data == null)
            return HandleResult(result);

        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }

    /// <summary>Get all documents for an appointment.</summary>
    [HttpGet("appointment/{appointmentId}")]
    public async Task<IActionResult> GetByAppointment(Guid appointmentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetAppointmentDocumentsQuery
        {
            AppointmentId = appointmentId,
            UserId = GetUserId(),
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Get all documents for a business, optionally filtered by type.</summary>
    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetByBusiness(Guid businessId, [FromQuery] string? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        DocumentType? typeFilter = null;
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<DocumentType>(type, true, out var parsed))
            typeFilter = parsed;

        var query = new GetBusinessDocumentsQuery
        {
            BusinessId = businessId,
            UserId = GetUserId(),
            TypeFilter = typeFilter,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Soft-delete a document by its ID.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteDocumentCommand
        {
            DocumentId = id,
            UserId = GetUserId(),
            IsAdmin = User.IsInRole("Admin")
        };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
