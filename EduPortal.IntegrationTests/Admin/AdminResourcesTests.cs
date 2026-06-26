using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EduPortal.Application.Common;
using EduPortal.Domain.Enums;
using DomainEntities = EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Persistence;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.IntegrationTests.Admin;

[Trait("Category", "Integration")]
public class AdminResourcesTests : IntegrationTestBase
{
    public AdminResourcesTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<Guid> SeedCategoryAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = new DomainEntities.Category("Admin Test Category", "admin-test-category");
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category.Id;
    }

    private async Task<Guid> SeedResourceAsync(Guid categoryId, Guid adminId, ResourceStatus status = ResourceStatus.Draft)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var resource = new DomainEntities.Resource("Admin Resource", "Description", ResourceType.Blog, 0m, categoryId, adminId)
        {
            Status = status,
            BlogContent = "Some content"
        };
        db.Resources.Add(resource);
        await db.SaveChangesAsync();
        return resource.Id;
    }

    [Fact]
    public async Task GetResources_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/admin/resources");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetResources_WithUserRole_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/admin/resources");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetResources_WithAdminRole_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/resources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateResource_WithAdmin_Returns201()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var categoryId = await SeedCategoryAsync();

        var response = await Client.PostAsJsonAsync("/api/admin/resources", new
        {
            Title = "New Resource",
            Description = "A new resource",
            Type = "Blog",
            CategoryId = categoryId,
            PricingType = "Free",
            BlogContent = "<p>New content</p>"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetResourceById_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var categoryId = await SeedCategoryAsync();
        var resourceId = await SeedResourceAsync(categoryId, adminId);

        var response = await Client.GetAsync($"/api/admin/resources/{resourceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetResourceById_NotFound_Returns404()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync($"/api/admin/resources/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateResource_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var categoryId = await SeedCategoryAsync();
        var resourceId = await SeedResourceAsync(categoryId, adminId);

        var response = await Client.PutAsJsonAsync($"/api/admin/resources/{resourceId}", new
        {
            Title = "Updated Resource",
            Description = "Updated description",
            Type = "Blog",
            CategoryId = categoryId,
            PricingType = "Free",
            BlogContent = "<p>Updated content</p>"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteResource_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var categoryId = await SeedCategoryAsync();
        var resourceId = await SeedResourceAsync(categoryId, adminId);

        var response = await Client.DeleteAsync($"/api/admin/resources/{resourceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PublishResource_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var categoryId = await SeedCategoryAsync();
        var resourceId = await SeedResourceAsync(categoryId, adminId, ResourceStatus.Draft);

        var response = await Client.PostAsync($"/api/admin/resources/{resourceId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ArchiveResource_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var categoryId = await SeedCategoryAsync();
        var resourceId = await SeedResourceAsync(categoryId, adminId, ResourceStatus.Published);

        var response = await Client.PostAsync($"/api/admin/resources/{resourceId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
