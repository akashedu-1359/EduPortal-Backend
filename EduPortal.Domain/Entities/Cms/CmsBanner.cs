using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities.Cms;

public class CmsBanner : BaseEntity
{
    public string Key { get; set; } = default!;
    public string ImageKey { get; set; } = default!;
    public string Headline { get; set; } = default!;
    public string? Subheadline { get; set; }
    public string? CtaText { get; set; }
    public string? CtaLink { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid UpdatedByAdminId { get; set; }
}
