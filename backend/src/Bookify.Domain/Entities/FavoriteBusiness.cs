using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

/// <summary>
/// A customer's favorite business (wishlist). Soft-deleted rows are ignored.
/// </summary>
public sealed class FavoriteBusiness : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid BusinessId { get; private set; }

    public User User { get; private set; } = null!;
    public Business Business { get; private set; } = null!;

    private FavoriteBusiness() { }

    public FavoriteBusiness(Guid userId, Guid businessId)
    {
        UserId = userId;
        BusinessId = businessId;
    }
}
