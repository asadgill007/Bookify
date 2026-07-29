using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Commands.Documents;

// ─── Upload Document ─────────────────────────────────────
public sealed record UploadDocumentCommand : IRequest<Result<Guid>>
{
    public Guid BusinessId { get; init; }
    public Guid UploadedByUserId { get; init; }
    public Guid? AppointmentId { get; init; }
    public Guid? ProviderId { get; init; }
    public DocumentType DocumentType { get; init; }
    public string FileName { get; init; } = string.Empty;
    public Stream Content { get; init; } = Stream.Null;
    public string ContentType { get; init; } = string.Empty;
}

public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx",
        ".xls", ".xlsx", ".txt", ".csv"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain", "text/csv"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.UploadedByUserId).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(200);

        RuleFor(x => x.FileName)
            .Must(fileName =>
            {
                var ext = Path.GetExtension(fileName);
                return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext);
            })
            .WithMessage("File type is not allowed. Supported: JPG, PNG, PDF, DOC, DOCX, XLS, XLSX, TXT, CSV.");

        RuleFor(x => x.ContentType)
            .Must(mime => AllowedMimeTypes.Contains(mime))
            .WithMessage("MIME type is not supported.");

        RuleFor(x => x.Content)
            .NotNull()
            .Must(s => s != Stream.Null)
            .WithMessage("File content is required.");

        RuleFor(x => x.Content.Length)
            .GreaterThan(0)
            .WithMessage("File is empty.");
    }
}

public sealed class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IVirusScanService _virusScan;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        IVirusScanService virusScan,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _virusScan = virusScan;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
            return Result<Guid>.Failure("Business not found.", "NOT_FOUND");

        // Virus scan the file
        var scanResult = await _virusScan.ScanAsync(request.Content, request.FileName, cancellationToken);
        if (!scanResult.IsClean)
        {
            _logger.LogWarning(
                "Virus scan rejected file {FileName} from business {BusinessId}: {Threat}",
                request.FileName, request.BusinessId, scanResult.ThreatDescription);
            return Result<Guid>.Failure("File was rejected by security scan.", "VIRUS_DETECTED");
        }

        var subDir = business.Slug ?? business.Id.ToString("N");

        var uploadResult = await _fileStorage.UploadAsync(
            request.Content,
            request.FileName,
            request.ContentType,
            subDir,
            cancellationToken);

        // Check for duplicate hash
        var isDuplicate = await _unitOfWork.Documents.HasDuplicateHashAsync(
            request.BusinessId, uploadResult.ContentHash, cancellationToken);

        if (isDuplicate)
        {
            // Delete the uploaded file since it's a duplicate
            await _fileStorage.DeleteAsync(uploadResult.StoragePath, cancellationToken);
            return Result<Guid>.Failure("A file with identical content already exists.", "DUPLICATE_FILE");
        }

        var extension = Path.GetExtension(request.FileName);
        var document = new Document(
            request.BusinessId,
            request.UploadedByUserId,
            request.DocumentType,
            Path.GetFileName(uploadResult.StoragePath),
            request.FileName,
            uploadResult.ContentType,
            extension,
            uploadResult.FileSize,
            uploadResult.StoragePath,
            uploadResult.ContentHash,
            request.AppointmentId,
            request.ProviderId,
            uploadResult.ThumbnailPath);

        await _unitOfWork.Documents.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Document {DocumentId} uploaded: {OriginalName} ({Size} bytes, type: {Type})",
            document.Id, request.FileName, uploadResult.FileSize, request.DocumentType);

        return Result<Guid>.Success(document.Id);
    }
}

// ─── Delete Document ─────────────────────────────────────
public sealed record DeleteDocumentCommand : IRequest<Result>
{
    public Guid DocumentId { get; init; }
    public Guid UserId { get; init; }
}

public sealed class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DeleteDocumentCommandHandler> _logger;

    public DeleteDocumentCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        ILogger<DeleteDocumentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document == null)
            return Result.Failure("Document not found.", "NOT_FOUND");

        // Soft delete in database
        document.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Delete physical file
        await _fileStorage.DeleteAsync(document.StoragePath, cancellationToken);
        if (document.ThumbnailPath != null)
            await _fileStorage.DeleteAsync(document.ThumbnailPath, cancellationToken);

        _logger.LogInformation("Document {DocumentId} deleted by user {UserId}", request.DocumentId, request.UserId);
        return Result.Success();
    }
}
