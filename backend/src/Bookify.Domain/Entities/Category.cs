using Bookify.Domain.Common;

namespace Bookify.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? IconName { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<BusinessCategory> BusinessCategories { get; private set; } = new List<BusinessCategory>();
    public ICollection<SubCategory> SubCategories { get; private set; } = new List<SubCategory>();

    private Category() { }

    public Category(string name, string slug, string? iconName, int displayOrder = 0)
    {
        SetName(name, slug);
        IconName = iconName;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void SetName(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Category slug cannot be empty.", nameof(slug));

        Name = name.Trim();
        Slug = slug.ToLowerInvariant().Trim();
        Touch();
    }

    public void Update(string? iconName, int displayOrder, bool isActive)
    {
        IconName = iconName;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        Touch();
    }
}

public sealed class SubCategory : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public Category Category { get; private set; } = null!;

    private SubCategory() { }

    public SubCategory(Guid categoryId, string name, string slug)
    {
        CategoryId = categoryId;
        SetName(name, slug);
        IsActive = true;
    }

    public void SetName(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("SubCategory name cannot be empty.", nameof(name));

        Name = name.Trim();
        Slug = slug.ToLowerInvariant().Trim();
        Touch();
    }
}

public sealed class BusinessCategory : BaseEntity
{
    public Guid BusinessId { get; private set; }
    public Guid CategoryId { get; private set; }

    public Business Business { get; private set; } = null!;
    public Category Category { get; private set; } = null!;

    private BusinessCategory() { }

    public BusinessCategory(Guid businessId, Guid categoryId)
    {
        BusinessId = businessId;
        CategoryId = categoryId;
    }
}
