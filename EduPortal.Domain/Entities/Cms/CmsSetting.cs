using EduPortal.Domain.Enums;

namespace EduPortal.Domain.Entities.Cms;

public class CmsSetting
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = string.Empty;
    public CmsDataType DataType { get; set; } = CmsDataType.String;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
