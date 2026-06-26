using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DomainEntities = EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Persistence;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.IntegrationTests.Admin;

[Trait("Category", "Integration")]
public class AdminUsersTests : IntegrationTestBase
{
    public AdminUsersTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetUsers_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithUserRole_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsers_WithSearchParam_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        await CreateTestUserAsync("searchuser@test.com", "Search User");

        var response = await Client.GetAsync("/api/admin/users?search=search");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateUserStatus_WithAdmin_ReturnsNoContent()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var (_, userId) = await CreateTestUserAsync("deactivate@test.com", "Deactivate User");

        var response = await Client.PatchAsJsonAsync($"/api/admin/users/{userId}/status", new
        {
            IsActive = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUserStatus_NonexistentUser_ReturnsError()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PatchAsJsonAsync($"/api/admin/users/{Guid.NewGuid()}/status", new
        {
            IsActive = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
