using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repositories;

/// <summary>Repository for customer favorites (wishlist).</summary>
public class FavoriteRepository : BaseRepository<FavoriteBusiness>, IFavoriteRepository
{
    public FavoriteRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<FavoriteBusiness>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(f => f.Business)
                .ThenInclude(b => b.BusinessCategories)
                    .ThenInclude(bc => bc.Category)
            .Include(f => f.Business)
                .ThenInclude(b => b.Images.Where(i => i.IsCover))
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(f => f.UserId == userId && f.BusinessId == businessId, cancellationToken);
    }

    public async Task<FavoriteBusiness?> GetAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(f => f.UserId == userId && f.BusinessId == businessId, cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetBusinessIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.BusinessId)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>Repository for persisted AI chat history.</summary>
public class ChatMessageRepository : BaseRepository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ChatMessage>> GetByUserAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>Repository for customer support tickets.</summary>
public class SupportTicketRepository : BaseRepository<SupportTicket>, ISupportTicketRepository
{
    public SupportTicketRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SupportTicket>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
