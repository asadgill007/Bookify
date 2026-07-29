namespace Bookify.Application.Interfaces;

/// <summary>
/// Virus scan hook interface for scanning uploaded files before they are stored.
/// Implementations can integrate with ClamAV, Windows Defender, or any antivirus API.
/// </summary>
public interface IVirusScanService
{
    /// <summary>Scan a file stream for threats. Returns clean status and any threat description.</summary>
    Task<VirusScanResult> ScanAsync(Stream fileContent, string fileName, CancellationToken cancellationToken = default);
}

/// <summary>Result of a virus scan operation.</summary>
public class VirusScanResult
{
    /// <summary>Whether the file is clean and safe to store.</summary>
    public bool IsClean { get; set; }

    /// <summary>Threat name or description if the file is infected.</summary>
    public string? ThreatDescription { get; set; }

    /// <summary>Name of the scanning engine used.</summary>
    public string? ScannerName { get; set; }

    public static VirusScanResult Clean(string? scannerName = null) => new()
    {
        IsClean = true,
        ScannerName = scannerName
    };

    public static VirusScanResult Infected(string threatDescription, string? scannerName = null) => new()
    {
        IsClean = false,
        ThreatDescription = threatDescription,
        ScannerName = scannerName
    };

    public static VirusScanResult NotAvailable() => new()
    {
        IsClean = true,
        ScannerName = null,
        ThreatDescription = null
    };
}
