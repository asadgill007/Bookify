using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

/// <summary>Document types supported by the system.</summary>
public enum DocumentType
{
    MedicalReport = 0,
    Prescription = 1,
    Contract = 2,
    Identity = 3,
    Certificate = 4,
    BeforePhoto = 5,
    AfterPhoto = 6,
    Other = 7
}

/// <summary>Represents a stored document/file in the system.</summary>
public sealed class Document : BaseEntity
{
    public Guid? AppointmentId { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid? ProviderId { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string FileName { get; private set; }
    public string OriginalFileName { get; private set; }
    public string ContentType { get; private set; }
    public string Extension { get; private set; }
    public long FileSize { get; private set; }
    public string StoragePath { get; private set; }
    public string? ThumbnailPath { get; private set; }
    public string ContentHash { get; private set; }
    public int Version { get; private set; }

    public Appointment? Appointment { get; private set; }
    public Business Business { get; private set; } = null!;
    public Provider? Provider { get; private set; }
    public User UploadedBy { get; private set; } = null!;

    private Document() { }

    public Document(
        Guid businessId,
        Guid uploadedByUserId,
        DocumentType documentType,
        string fileName,
        string originalFileName,
        string contentType,
        string extension,
        long fileSize,
        string storagePath,
        string contentHash,
        Guid? appointmentId = null,
        Guid? providerId = null,
        string? thumbnailPath = null,
        int version = 1)
    {
        BusinessId = businessId;
        UploadedByUserId = uploadedByUserId;
        DocumentType = documentType;
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        Extension = extension;
        FileSize = fileSize;
        StoragePath = storagePath;
        ContentHash = contentHash;
        AppointmentId = appointmentId;
        ProviderId = providerId;
        ThumbnailPath = thumbnailPath;
        Version = version;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Touch();
    }

    public void UpdateStoragePath(string newPath, string? newThumbnailPath = null)
    {
        StoragePath = newPath;
        if (newThumbnailPath != null)
            ThumbnailPath = newThumbnailPath;
        Version++;
        Touch();
    }
}
