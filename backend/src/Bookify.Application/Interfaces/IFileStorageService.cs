namespace Bookify.Application.Interfaces;

/// <summary>Result of a file upload operation.</summary>
public class FileUploadResult
{
    public string StoragePath { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public long FileSize { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>Result of a file download operation with stream data.</summary>
public class FileDownloadResult : IDisposable
{
    public Stream Content { get; set; } = Stream.Null;
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }

    public void Dispose() => Content?.Dispose();
}

/// <summary>Abstract file storage service supporting local, Azure Blob, and AWS S3 implementations.</summary>
public interface IFileStorageService
{
    /// <summary>Upload a file and return the storage path.</summary>
    Task<FileUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string? subDirectory = null,
        CancellationToken cancellationToken = default);

    /// <summary>Download a file by its storage path.</summary>
    Task<FileDownloadResult> DownloadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>Delete a file from storage.</summary>
    Task<bool> DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>Generate a signed/access URL for temporary file access. Returns null if not supported.</summary>
    Task<string?> GetSignedUrlAsync(
        string storagePath,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>Check if a file exists at the given path.</summary>
    Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}
