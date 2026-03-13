using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<Resource> Resources { get; private set; } = new List<Resource>();

    private Category() { }

    public Category(string name, string slug, string? description = null)
    {
        Name = name;
        Slug = slug;
        Description = description;
    }
}
