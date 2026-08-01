using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // TRACKED on purpose: auth flows mutate the returned user (RecordLogin,
        // LinkGoogleAccount, password change) and rely on SaveChangesAsync.
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim(), cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(u => u.Email == email.ToLowerInvariant().Trim(), cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetFilteredAsync(
        UserRole? roleFilter,
        bool? suspendedFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (roleFilter.HasValue)
            query = query.Where(u => u.Role == roleFilter.Value);

        if (suspendedFilter.HasValue)
            query = query.Where(u => u.IsSuspended == suspendedFilter.Value);

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFilteredCountAsync(
        UserRole? roleFilter,
        bool? suspendedFilter,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (roleFilter.HasValue)
            query = query.Where(u => u.Role == roleFilter.Value);

        if (suspendedFilter.HasValue)
            query = query.Where(u => u.IsSuspended == suspendedFilter.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<User?> GetWithRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByGoogleSubjectAsync(string googleSubject, CancellationToken cancellationToken = default)
    {
        // TRACKED so Google account linking persists via SaveChangesAsync.
        return await DbSet
            .FirstOrDefaultAsync(u => u.GoogleSubject == googleSubject, cancellationToken);
    }
}
