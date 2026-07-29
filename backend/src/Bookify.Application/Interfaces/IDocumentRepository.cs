using Bookify.Domain.Entities;

namespace Bookify.Application.Interfaces;

/// <summary>Repository for document storage operations with DB-level filtered queries.</summary>
public interface IDocumentRepository : IRepository<Document>
{
    /// <summary>Get documents for an appointment with pagination.</summary>
    Task<IReadOnlyList<Document>> GetByAppointmentIdAsync(
        Guid appointmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Count documents for an appointment.</summary>
    Task<int> GetAppointmentDocumentCountAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    /// <summary>Get documents for a business with pagination.</summary>
    Task<IReadOnlyList<Document>> GetByBusinessIdAsync(
        Guid businessId,
        DocumentType? typeFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Count documents for a business (with optional type filter).</summary>
    Task<int> GetBusinessDocumentCountAsync(Guid businessId, DocumentType? typeFilter, CancellationToken cancellationToken = default);

    /// <summary>Check if a document with the same content hash already exists for the business.</summary>
    Task<bool> HasDuplicateHashAsync(Guid businessId, string contentHash, CancellationToken cancellationToken = default);
}
