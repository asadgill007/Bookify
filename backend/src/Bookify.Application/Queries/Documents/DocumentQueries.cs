using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using MediatR;

namespace Bookify.Application.Queries.Documents;

// ─── Download Document ───────────────────────────────────
public sealed record DownloadDocumentQuery : IRequest<Result<FileDownloadDto>>
{
    public Guid DocumentId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, Result<FileDownloadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;

    public DownloadDocumentQueryHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
    }

    public async Task<Result<FileDownloadDto>> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document == null)
            return Result<FileDownloadDto>.Failure("Document not found.", "NOT_FOUND");

        var download = await _fileStorage.DownloadAsync(document.StoragePath, cancellationToken);

        return Result<FileDownloadDto>.Success(new FileDownloadDto
        {
            Content = download.Content,
            ContentType = download.ContentType,
            FileName = document.OriginalFileName,
            FileSize = download.FileSize
        });
    }
}

// ─── Get Appointment Documents ──────────────────────────
public sealed record GetAppointmentDocumentsQuery : IRequest<Result<PaginatedList<DocumentDto>>>
{
    public Guid AppointmentId { get; init; }
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetAppointmentDocumentsQueryHandler : IRequestHandler<GetAppointmentDocumentsQuery, Result<PaginatedList<DocumentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentDocumentsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<DocumentDto>>> Handle(GetAppointmentDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _unitOfWork.Documents.GetByAppointmentIdAsync(
            request.AppointmentId, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Documents.GetAppointmentDocumentCountAsync(request.AppointmentId, cancellationToken);

        var items = documents.Select(MapToDto).ToList();
        return Result<PaginatedList<DocumentDto>>.Success(new PaginatedList<DocumentDto>(items, request.Page, request.PageSize, total));
    }

    private static DocumentDto MapToDto(Document d) => new()
    {
        Id = d.Id,
        DocumentType = d.DocumentType.ToString(),
        FileName = d.OriginalFileName,
        ContentType = d.ContentType,
        FileSize = d.FileSize,
        Version = d.Version,
        UploadedByName = d.UploadedBy != null ? $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}" : "",
        CreatedAt = d.CreatedAt
    };
}

// ─── Get Business Documents ─────────────────────────────
public sealed record GetBusinessDocumentsQuery : IRequest<Result<PaginatedList<DocumentDto>>>
{
    public Guid BusinessId { get; init; }
    public Guid UserId { get; init; }
    public DocumentType? TypeFilter { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetBusinessDocumentsQueryHandler : IRequestHandler<GetBusinessDocumentsQuery, Result<PaginatedList<DocumentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBusinessDocumentsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<DocumentDto>>> Handle(GetBusinessDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _unitOfWork.Documents.GetByBusinessIdAsync(
            request.BusinessId, request.TypeFilter, request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Documents.GetBusinessDocumentCountAsync(
            request.BusinessId, request.TypeFilter, cancellationToken);

        var items = documents.Select(d => new DocumentDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType.ToString(),
            FileName = d.OriginalFileName,
            ContentType = d.ContentType,
            FileSize = d.FileSize,
            Version = d.Version,
            AppointmentId = d.AppointmentId,
            UploadedByName = d.UploadedBy != null ? $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}" : "",
            CreatedAt = d.CreatedAt
        }).ToList();

        return Result<PaginatedList<DocumentDto>>.Success(new PaginatedList<DocumentDto>(items, request.Page, request.PageSize, total));
    }
}

// ─── DTOs ────────────────────────────────────────────────
public class DocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Version { get; set; }
    public Guid? AppointmentId { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class FileDownloadDto : IDisposable
{
    public Stream Content { get; set; } = Stream.Null;
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }

    public void Dispose() => Content?.Dispose();
}
