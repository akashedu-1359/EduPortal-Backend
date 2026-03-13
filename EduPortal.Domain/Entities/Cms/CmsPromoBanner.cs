using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities.Cms;

public class CmsPromoBanner : BaseEntity
{
    public string ImageKey { get; set; } = default!;
    public string? Title { get; set; }
    public string? Link { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
