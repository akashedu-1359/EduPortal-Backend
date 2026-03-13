using EduPortal.Application.Common;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Entities.Cms;
using EduPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        await SeedRolesAndPermissionsAsync(db, logger);
        await SeedCmsAsync(db, logger);
        await db.SaveChangesAsync();
    }

    private static async Task SeedRolesAndPermissionsAsync(AppDbContext db, ILogger logger)
    {
        if (await db.Roles.AnyAsync()) return;

        logger.LogInformation("Seeding roles and permissions...");

        var roles = new[]
        {
            new Role(RoleConstants.SuperAdminId, RoleConstants.SuperAdmin, "Full system access"),
            new Role(RoleConstants.AdminId, RoleConstants.Admin, "Platform administration"),
            new Role(RoleConstants.ContentManagerId, RoleConstants.ContentManager, "Manage content resources"),
            new Role(RoleConstants.ExamManagerId, RoleConstants.ExamManager, "Manage exams and questions"),
            new Role(RoleConstants.AnalystId, RoleConstants.Analyst, "View analytics and reports"),
            new Role(RoleConstants.UserId, RoleConstants.User, "Student portal access"),
        };
        db.Roles.AddRange(roles);

        var permissions = new[]
        {
            new Permission(1, PermissionKeys.ManageUsers, "Manage roles and users"),
            new Permission(2, PermissionKeys.ContentWrite, "Create and edit resources"),
            new Permission(3, PermissionKeys.ExamManage, "Create and manage exams"),
            new Permission(4, PermissionKeys.CmsManage, "Manage CMS content"),
            new Permission(5, PermissionKeys.AnalyticsView, "View analytics dashboard"),
            new Permission(6, PermissionKeys.ManageFeatureFlags, "Manage feature flags"),
        };
        db.Permissions.AddRange(permissions);

        // SuperAdmin: all permissions
        var superAdminPerms = permissions.Select(p => new RolePermission(RoleConstants.SuperAdminId, p.Id));
        // Admin: all permissions
        var adminPerms = permissions.Select(p => new RolePermission(RoleConstants.AdminId, p.Id));
        // ContentManager: content:write only
        var cmPerms = new[] { new RolePermission(RoleConstants.ContentManagerId, 2) };
        // ExamManager: exams:manage only
        var emPerms = new[] { new RolePermission(RoleConstants.ExamManagerId, 3) };
        // Analyst: analytics:view only
        var analystPerms = new[] { new RolePermission(RoleConstants.AnalystId, 5) };

        db.RolePermissions.AddRange(superAdminPerms);
        db.RolePermissions.AddRange(adminPerms);
        db.RolePermissions.AddRange(cmPerms);
        db.RolePermissions.AddRange(emPerms);
        db.RolePermissions.AddRange(analystPerms);
    }

    private static async Task SeedCmsAsync(AppDbContext db, ILogger logger)
    {
        if (await db.CmsSections.AnyAsync()) return;

        logger.LogInformation("Seeding CMS defaults...");

        var sections = new[]
        {
            new CmsSection { Key = "show_hero", IsVisible = true, Label = "Hero Banner" },
            new CmsSection { Key = "show_featured_resources", IsVisible = true, Label = "Featured Resources" },
            new CmsSection { Key = "show_testimonials", IsVisible = true, Label = "Testimonials" },
            new CmsSection { Key = "show_promo_banner", IsVisible = false, Label = "Promo Banner" },
            new CmsSection { Key = "show_stats", IsVisible = true, Label = "Stats Bar" },
        };
        db.CmsSections.AddRange(sections);

        var settings = new[]
        {
            new CmsSetting { Key = "site_name", Value = "EduPortal", DataType = CmsDataType.String, Description = "Platform display name" },
            new CmsSetting { Key = "support_email", Value = "support@eduportal.com", DataType = CmsDataType.String, Description = "Support contact email" },
            new CmsSetting { Key = "currency", Value = "INR", DataType = CmsDataType.String, Description = "Default currency code" },
        };
        db.CmsSettings.AddRange(settings);

        var flags = new[]
        {
            new CmsFeatureFlag { Key = "enable_payments", IsEnabled = true, Description = "Enable payment gateway" },
            new CmsFeatureFlag { Key = "enable_exams", IsEnabled = true, Description = "Enable exam module" },
            new CmsFeatureFlag { Key = "enable_certificates", IsEnabled = true, Description = "Enable certificate generation" },
            new CmsFeatureFlag { Key = "enable_google_auth", IsEnabled = true, Description = "Enable Google OAuth login" },
            new CmsFeatureFlag { Key = "maintenance_mode", IsEnabled = false, Description = "Show maintenance page" },
        };
        db.CmsFeatureFlags.AddRange(flags);

        var pages = new[]
        {
            new CmsPage { Slug = "about-us", Title = "About Us", Content = "", IsPublished = false },
            new CmsPage { Slug = "contact", Title = "Contact", Content = "", IsPublished = false },
            new CmsPage { Slug = "privacy", Title = "Privacy Policy", Content = "", IsPublished = false },
            new CmsPage { Slug = "terms", Title = "Terms & Conditions", Content = "", IsPublished = false },
            new CmsPage { Slug = "faq", Title = "FAQ", Content = "", IsPublished = false },
            new CmsPage { Slug = "help", Title = "Help Center", Content = "", IsPublished = false },
        };
        db.CmsPages.AddRange(pages);

        db.CmsFooters.Add(new CmsFooter
        {
            CompanyText = "Empowering learners worldwide.",
            CopyrightText = $"© {DateTime.UtcNow.Year} EduPortal. All rights reserved.",
        });
    }
}
