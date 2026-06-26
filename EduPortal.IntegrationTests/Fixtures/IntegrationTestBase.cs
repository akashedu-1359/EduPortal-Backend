using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using EduPortal.Application.Common;
using EduPortal.Infrastructure.Persistence;
using DomainUser = EduPortal.Domain.Entities.User;
using DomainUserRole = EduPortal.Domain.Entities.UserRole;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EduPortal.IntegrationTests.Fixtures;

[Trait("Category", "Integration")]
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    private const string TestJwtSecret = "ThisIsATestSecretKeyThatMustBe32CharsLong!!";
    private const string TestIssuer = "EduPortal.Tests";
    private const string TestAudience = "EduPortal.Tests";

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public virtual Task InitializeAsync() => ResetDatabaseAsync();

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Clean user-created data but keep seeded roles/permissions/CMS
        db.AttemptAnswers.RemoveRange(db.AttemptAnswers);
        db.ExamAttempts.RemoveRange(db.ExamAttempts);
        db.Certificates.RemoveRange(db.Certificates);
        db.Enrollments.RemoveRange(db.Enrollments);
        db.Orders.RemoveRange(db.Orders);
        db.Questions.RemoveRange(db.Questions);
        db.Exams.RemoveRange(db.Exams);
        db.Resources.RemoveRange(db.Resources);
        db.Categories.RemoveRange(db.Categories);
        db.RefreshTokens.RemoveRange(db.RefreshTokens);
        db.AuditLogs.RemoveRange(db.AuditLogs);

        // Remove non-seeded users (keep none since users are test-specific)
        db.UserRoles.RemoveRange(db.UserRoles);
        db.Users.RemoveRange(db.Users);

        await db.SaveChangesAsync();
    }

    protected string GenerateJwtToken(Guid userId, string email, string role, IEnumerable<string>? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        };

        if (permissions != null)
        {
            foreach (var perm in permissions)
                claims.Add(new Claim("permissions", perm));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected void AuthenticateAsUser(Guid userId, string email = "user@test.com")
    {
        var token = GenerateJwtToken(userId, email, RoleConstants.User);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void AuthenticateAsAdmin(Guid userId, string email = "admin@test.com")
    {
        var allPermissions = new[]
        {
            PermissionKeys.ManageUsers,
            PermissionKeys.ContentWrite,
            PermissionKeys.ExamManage,
            PermissionKeys.CmsManage,
            PermissionKeys.AnalyticsView,
            PermissionKeys.ManageFeatureFlags,
        };
        var token = GenerateJwtToken(userId, email, RoleConstants.SuperAdmin, allPermissions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void AuthenticateWithPermissions(Guid userId, string role, string email, params string[] permissions)
    {
        var token = GenerateJwtToken(userId, email, role, permissions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task<(DomainUser user, Guid userId)> CreateTestUserAsync(
        string email = "testuser@test.com",
        string fullName = "Test User",
        string passwordHash = "hashed_password",
        int roleId = 6 /* User */)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = DomainUser.CreateLocal(email, passwordHash, fullName);
        db.Users.Add(user);
        db.UserRoles.Add(new DomainUserRole(user.Id, roleId));
        await db.SaveChangesAsync();

        return (user, user.Id);
    }

    protected async Task<(DomainUser admin, Guid adminId)> CreateTestAdminAsync(
        string email = "admin@test.com",
        string fullName = "Test Admin")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var admin = DomainUser.CreateLocal(email, "hashed_password", fullName);
        db.Users.Add(admin);
        db.UserRoles.Add(new DomainUserRole(admin.Id, RoleConstants.SuperAdminId));
        await db.SaveChangesAsync();

        return (admin, admin.Id);
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();
}
