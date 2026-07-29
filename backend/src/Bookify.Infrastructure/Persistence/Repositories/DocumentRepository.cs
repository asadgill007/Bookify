using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

public class DocumentRepository : BaseRepository<Document>, IDocumentRepository
{
    public DocumentRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Document>> GetByAppointmentIdAsync(
        Guid appointmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.UploadedBy)
            .Where(d => d.AppointmentId == appointmentId)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAppointmentDocumentCountAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(d => d.AppointmentId == appointmentId, cancellationToken);

    public async Task<IReadOnlyList<Document>> GetByBusinessIdAsync(
        Guid businessId,
        DocumentType? typeFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(d => d.UploadedBy)
            .Include(d => d.Appointment)
            .Where(d => d.BusinessId == businessId);

        if (typeFilter.HasValue)
            query = query.Where(d => d.DocumentType == typeFilter.Value);

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetBusinessDocumentCountAsync(Guid businessId, DocumentType? typeFilter, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(d => d.BusinessId == businessId);

        if (typeFilter.HasValue)
            query = query.Where(d => d.DocumentType == typeFilter.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> HasDuplicateHashAsync(Guid businessId, string contentHash, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(d => d.BusinessId == businessId && d.ContentHash == contentHash, cancellationToken);
}
