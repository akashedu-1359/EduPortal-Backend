namespace EduPortal.Domain.Entities.Cms;

public class CmsFeatureFlag
{
    public string Key { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
