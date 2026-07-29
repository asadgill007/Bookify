using System.Security.Cryptography;
using Bookify.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Local file system implementation of <see cref="IFileStorageService"/>.
/// Files are stored under the configured base path with a date-based directory structure.
/// Configuration key: "FileStorage:Local:BasePath" (defaults to "Uploads" in the current directory).
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    /// <summary>
    /// Initializes a new instance with a configurable base path.
    /// </summary>
    /// <param name="configuration">Application configuration for reading "FileStorage:Local:BasePath".</param>
    /// <param name="logger">Logger instance.</param>
    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        var configuredPath = configuration.GetValue<string>("FileStorage:Local:BasePath");
        _basePath = Path.GetFullPath(configuredPath ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads"));
        _logger = logger;
        Directory.CreateDirectory(_basePath);
    }

    /// <inheritdoc />
    public async Task<FileUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string? subDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var dateDir = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var relativeDir = subDirectory != null
            ? $"{dateDir}/{subDirectory}"
            : dateDir;

        var storageDir = Path.Combine(_basePath, relativeDir);
        Directory.CreateDirectory(storageDir);

        var storageName = $"{Guid.NewGuid()}{extension}";
        var storagePath = $"{relativeDir}/{storageName}";
        var fullPath = Path.Combine(_basePath, storagePath);

        // Compute content hash from stream
        string contentHash;
        using (var sha256 = SHA256.Create())
        {
            content.Position = 0;
            var hashBytes = await sha256.ComputeHashAsync(content, cancellationToken);
            contentHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        // Write file to disk
        content.Position = 0;
        await using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var fileInfo = new FileInfo(fullPath);

        _logger.LogInformation(
            "Uploaded file {StoragePath} ({FileSize} bytes, hash: {Hash})",
            storagePath, fileInfo.Length, contentHash);

        return new FileUploadResult
        {
            StoragePath = storagePath,
            FileSize = fileInfo.Length,
            ContentHash = contentHash,
            ContentType = contentType
        };
    }

    /// <inheritdoc />
    public async Task<FileDownloadResult> DownloadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found.", storagePath);

        var memoryStream = new MemoryStream();
        await using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true))
        {
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
        }

        memoryStream.Position = 0;

        return new FileDownloadResult
        {
            Content = memoryStream,
            ContentType = GetContentType(storagePath),
            FileName = Path.GetFileName(storagePath),
            FileSize = memoryStream.Length
        };
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        _logger.LogInformation("Deleted file {StoragePath}", storagePath);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<string?> GetSignedUrlAsync(string storagePath, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        // Local storage does not support signed URLs; return a relative path for direct access.
        return Task.FromResult<string?>($"/uploads/{storagePath}");
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    /// <summary>Maps file extensions to MIME content types.</summary>
    private static string GetContentType(string path)
    {
        var extension = Path.GetExtension(path)?.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
