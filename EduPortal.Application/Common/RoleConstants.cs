namespace EduPortal.Application.Common;

public static class RoleConstants
{
    public const int SuperAdminId = 1;
    public const int AdminId = 2;
    public const int ContentManagerId = 3;
    public const int ExamManagerId = 4;
    public const int AnalystId = 5;
    public const int UserId = 6;

    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string ContentManager = "ContentManager";
    public const string ExamManager = "ExamManager";
    public const string Analyst = "Analyst";
    public const string User = "User";
}

public static class PermissionKeys
{
    public const string ManageUsers = "users:manage";
    public const string ContentWrite = "content:write";
    public const string ExamManage = "exams:manage";
    public const string CmsManage = "cms:manage";
    public const string AnalyticsView = "analytics:view";
    public const string ManageFeatureFlags = "flags:manage";
}
