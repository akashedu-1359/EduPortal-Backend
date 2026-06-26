using System.Net;
using EduPortal.Application.Common;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EduPortal.IntegrationTests.Authorization;

[Trait("Category", "Integration")]
public class PermissionTests : IntegrationTestBase
{
    public PermissionTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AdminResources_WithoutContentWritePermission_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        // Authenticate with AnalyticsView only — no ContentWrite
        AuthenticateWithPermissions(userId, RoleConstants.Analyst, "analyst@test.com", PermissionKeys.AnalyticsView);

        var response = await Client.GetAsync("/api/admin/resources");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminResources_WithContentWritePermission_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateWithPermissions(adminId, RoleConstants.ContentManager, "cm@test.com", PermissionKeys.ContentWrite);

        var response = await Client.GetAsync("/api/admin/resources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminCategories_WithoutContentWritePermission_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateWithPermissions(userId, RoleConstants.ExamManager, "em@test.com", PermissionKeys.ExamManage);

        var response = await Client.GetAsync("/api/admin/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminExams_WithoutExamManagePermission_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateWithPermissions(userId, RoleConstants.ContentManager, "cm@test.com", PermissionKeys.ContentWrite);

        var response = await Client.GetAsync("/api/admin/exams");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminExams_WithExamManagePermission_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateWithPermissions(adminId, RoleConstants.ExamManager, "em@test.com", PermissionKeys.ExamManage);

        var response = await Client.GetAsync("/api/admin/exams");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminUsers_WithoutUserManagePermission_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateWithPermissions(userId, RoleConstants.ContentManager, "cm@test.com", PermissionKeys.ContentWrite);

        var response = await Client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminUsers_WithUserManagePermission_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateWithPermissions(adminId, RoleConstants.Admin, "adminuser@test.com", PermissionKeys.ManageUsers);

        var response = await Client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminAnalytics_WithoutAnalyticsViewPermission_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateWithPermissions(userId, RoleConstants.ContentManager, "cm@test.com", PermissionKeys.ContentWrite);

        var response = await Client.GetAsync("/api/admin/analytics/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminAnalytics_WithAnalyticsViewPermission_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateWithPermissions(adminId, RoleConstants.Analyst, "analyst@test.com", PermissionKeys.AnalyticsView);

        var response = await Client.GetAsync("/api/admin/analytics/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminCms_WithoutCmsManagePermission_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateWithPermissions(userId, RoleConstants.ExamManager, "em@test.com", PermissionKeys.ExamManage);

        var response = await Client.GetAsync("/api/admin/cms/banners");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminCms_WithCmsManagePermission_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateWithPermissions(adminId, RoleConstants.Admin, "cmsadmin@test.com", PermissionKeys.CmsManage);

        var response = await Client.GetAsync("/api/admin/cms/banners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UserEndpoints_WithRegularUserToken_Returns200()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/user/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UserEndpoints_WithoutToken_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/user/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublicEndpoints_WithoutToken_Returns200()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/resources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PaymentEndpoints_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.PostAsync("/api/payments/initiate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
