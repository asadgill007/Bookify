using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public sealed class ReviewVote : BaseEntity
{
    public Guid ReviewId { get; private set; }
    public Guid CustomerId { get; private set; }
    public bool IsHelpful { get; private set; }

    public Review Review { get; private set; } = null!;
    public User Customer { get; private set; } = null!;

    private ReviewVote() { }

    public ReviewVote(Guid reviewId, Guid customerId, bool isHelpful)
    {
        ReviewId = reviewId;
        CustomerId = customerId;
        IsHelpful = isHelpful;
    }

    public void Toggle(bool isHelpful)
    {
        IsHelpful = isHelpful;
        Touch();
    }
}
