namespace EduPortal.Domain.Entities.Cms;

public class CmsFooter
{
    public int Id { get; private set; } = 1;
    public string CompanyText { get; set; } = string.Empty;
    public string CopyrightText { get; set; } = string.Empty;
    public string SocialLinksJson { get; set; } = "[]";
    public string ColumnsJson { get; set; } = "[]";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
