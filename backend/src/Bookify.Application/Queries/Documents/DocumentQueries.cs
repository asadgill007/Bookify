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

    /// <summary>True when the requesting user has the Admin role.</summary>
    public bool IsAdmin { get; init; }
}

public sealed class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, Result<FileDownloadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IPermissionService _permissionService;

    public DownloadDocumentQueryHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _permissionService = permissionService;
    }

    public async Task<Result<FileDownloadDto>> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document == null)
            return Result<FileDownloadDto>.Failure("Document not found.", "NOT_FOUND");

        // Only admins, the business owner, its providers, the uploader, or (for
        // appointment-linked documents) the appointment's customer may download.
        var canManageBusiness =
            await _permissionService.CanManageBusinessAsync(request.UserId, document.BusinessId, cancellationToken);
        var isProvider =
            await _permissionService.IsProviderForBusinessAsync(request.UserId, document.BusinessId, cancellationToken);
        var isUploader = document.UploadedByUserId == request.UserId;
        var isAppointmentParty = document.AppointmentId.HasValue &&
            await _permissionService.CanAccessAppointmentAsync(request.UserId, document.AppointmentId.Value, cancellationToken);

        if (!request.IsAdmin && !canManageBusiness && !isProvider && !isUploader && !isAppointmentParty)
            return Result<FileDownloadDto>.Failure("You do not have permission to download this document.", "FORBIDDEN");

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

    /// <summary>True when the requesting user has the Admin role.</summary>
    public bool IsAdmin { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetAppointmentDocumentsQueryHandler : IRequestHandler<GetAppointmentDocumentsQuery, Result<PaginatedList<DocumentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;

    public GetAppointmentDocumentsQueryHandler(IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task<Result<PaginatedList<DocumentDto>>> Handle(GetAppointmentDocumentsQuery request, CancellationToken cancellationToken)
    {
        // Resolve the appointment to authorize business-level access.
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
            return Result<PaginatedList<DocumentDto>>.Failure("Appointment not found.", "NOT_FOUND");

        var canManageBusiness =
            await _permissionService.CanManageBusinessAsync(request.UserId, appointment.BusinessId, cancellationToken);
        var isProvider =
            await _permissionService.IsProviderForBusinessAsync(request.UserId, appointment.BusinessId, cancellationToken);

        if (!request.IsAdmin
            && !canManageBusiness
            && !isProvider
            && !await _permissionService.CanAccessAppointmentAsync(request.UserId, request.AppointmentId, cancellationToken))
        {
            return Result<PaginatedList<DocumentDto>>.Failure("You do not have permission to view these documents.", "FORBIDDEN");
        }

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

    /// <summary>True when the requesting user has the Admin role.</summary>
    public bool IsAdmin { get; init; }
    public DocumentType? TypeFilter { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetBusinessDocumentsQueryHandler : IRequestHandler<GetBusinessDocumentsQuery, Result<PaginatedList<DocumentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;

    public GetBusinessDocumentsQueryHandler(IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task<Result<PaginatedList<DocumentDto>>> Handle(GetBusinessDocumentsQuery request, CancellationToken cancellationToken)
    {
        // Only admins, the business owner, or its providers may list business documents.
        if (!request.IsAdmin
            && !await _permissionService.CanManageBusinessAsync(request.UserId, request.BusinessId, cancellationToken)
            && !await _permissionService.IsProviderForBusinessAsync(request.UserId, request.BusinessId, cancellationToken))
        {
            return Result<PaginatedList<DocumentDto>>.Failure("You do not have permission to view documents for this business.", "FORBIDDEN");
        }

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
