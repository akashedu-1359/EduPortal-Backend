using EduPortal.Domain.Entities;
using EduPortal.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Core
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // CMS
    public DbSet<CmsBanner> CmsBanners => Set<CmsBanner>();
    public DbSet<CmsPage> CmsPages => Set<CmsPage>();
    public DbSet<CmsFaq> CmsFaqs => Set<CmsFaq>();
    public DbSet<CmsFooter> CmsFooters => Set<CmsFooter>();
    public DbSet<CmsSection> CmsSections => Set<CmsSection>();
    public DbSet<CmsSetting> CmsSettings => Set<CmsSetting>();
    public DbSet<CmsFeatureFlag> CmsFeatureFlags => Set<CmsFeatureFlag>();
    public DbSet<CmsPromoBanner> CmsPromoBanners => Set<CmsPromoBanner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
