using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities.Cms;

public class CmsFaq : BaseEntity
{
    public string Question { get; set; } = default!;
    public string Answer { get; set; } = default!;
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}
