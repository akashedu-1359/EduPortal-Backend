using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;

namespace EduPortal.Domain.Entities;

public class Resource : BaseEntity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public ResourceType ResourceType { get; set; }
    public string? FileKey { get; set; }
    public string? ExternalUrl { get; set; }
    public string? BlogContent { get; set; }
    public string? ThumbnailKey { get; set; }
    public decimal Price { get; set; }
    public ResourceStatus Status { get; set; } = ResourceStatus.Draft;
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public Guid CreatedByAdminId { get; set; }
    public bool IsDeleted { get; set; }

    public Category Category { get; private set; } = default!;
    public User CreatedByAdmin { get; private set; } = default!;
    public ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();

    private Resource() { }

    public Resource(string title, string description, ResourceType type, decimal price, Guid categoryId, Guid adminId)
    {
        Title = title;
        Description = description;
        ResourceType = type;
        Price = price;
        CategoryId = categoryId;
        CreatedByAdminId = adminId;
    }
}
