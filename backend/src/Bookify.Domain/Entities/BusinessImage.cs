using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public sealed class BusinessImage : BaseEntity
{
    public Guid BusinessId { get; private set; }
    public string Url { get; private set; }
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsCover { get; private set; }

    public Business Business { get; private set; } = null!;

    private BusinessImage() { }

    public BusinessImage(Guid businessId, string url, string? altText, int displayOrder = 0, bool isCover = false)
    {
        BusinessId = businessId;
        SetUrl(url);
        AltText = altText?.Trim();
        DisplayOrder = displayOrder;
        IsCover = isCover;
    }

    public void SetUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be empty.", nameof(url));

        Url = url;
        Touch();
    }

    public void SetAsCover(bool isCover)
    {
        IsCover = isCover;
        Touch();
    }

    public void UpdateOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        Touch();
    }
}
