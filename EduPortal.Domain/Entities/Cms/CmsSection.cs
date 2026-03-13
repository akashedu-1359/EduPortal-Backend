using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities.Cms;

public class CmsSection : BaseEntity
{
    public string Key { get; set; } = default!;
    public bool IsVisible { get; set; } = true;
    public string Label { get; set; } = default!;
}
