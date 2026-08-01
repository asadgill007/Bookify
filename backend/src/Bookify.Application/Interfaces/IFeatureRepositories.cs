using Bookify.Domain.Common;
using Bookify.Domain.Entities;

namespace Bookify.Application.Interfaces;

/// <summary>Repository for customer favorites (wishlist).</summary>
public interface IFavoriteRepository : IRepository<FavoriteBusiness>
{
    Task<IReadOnlyList<FavoriteBusiness>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default);
    Task<FavoriteBusiness?> GetAsync(Guid userId, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetBusinessIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>Repository for persisted AI chat history.</summary>
public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<IReadOnlyList<ChatMessage>> GetByUserAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default);
}

/// <summary>Repository for customer support tickets.</summary>
public interface ISupportTicketRepository : IRepository<SupportTicket>
{
    Task<IReadOnlyList<SupportTicket>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
