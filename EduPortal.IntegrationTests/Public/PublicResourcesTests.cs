using System.Net;
using System.Text.Json;
using EduPortal.Domain.Enums;
using DomainEntities = EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Persistence;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.IntegrationTests.Public;

[Trait("Category", "Integration")]
public class PublicResourcesTests : IntegrationTestBase
{
    public PublicResourcesTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<(Guid categoryId, Guid adminId)> SeedResourcePrerequisitesAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var admin = DomainEntities.User.CreateLocal("resourceadmin@test.com", "hash", "Resource Admin");
        db.Users.Add(admin);

        var category = new DomainEntities.Category("Test Category", "test-category", "A test category");
        db.Categories.Add(category);

        await db.SaveChangesAsync();
        return (category.Id, admin.Id);
    }

    private async Task<Guid> SeedPublishedResourceAsync(Guid categoryId, Guid adminId, string title = "Test Resource", decimal price = 0m)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var resource = new DomainEntities.Resource(title, "A test resource description", ResourceType.Blog, price, categoryId, adminId)
        {
            Status = ResourceStatus.Published,
            BlogContent = "<p>Test blog content</p>"
        };
        db.Resources.Add(resource);
        await db.SaveChangesAsync();
        return resource.Id;
    }

    [Fact]
    public async Task GetResources_ReturnsSuccessWithPaginatedData()
    {
        var (catId, adminId) = await SeedResourcePrerequisitesAsync();
        await SeedPublishedResourceAsync(catId, adminId);

        var response = await Client.GetAsync("/api/resources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetResources_WithPagination_RespectsPageSize()
    {
        var (catId, adminId) = await SeedResourcePrerequisitesAsync();
        for (int i = 0; i < 3; i++)
            await SeedPublishedResourceAsync(catId, adminId, $"Resource {i}");

        var response = await Client.GetAsync("/api/resources?pageNumber=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetResources_WithTypeFilter_ReturnsFilteredResults()
    {
        var (catId, adminId) = await SeedResourcePrerequisitesAsync();
        await SeedPublishedResourceAsync(catId, adminId);

        var response = await Client.GetAsync("/api/resources?type=Blog");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetResourceBySlug_WhenNotFound_Returns404()
    {
        var response = await Client.GetAsync("/api/resources/nonexistent-slug");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetResources_ReturnsEmptyWhenNoPublishedResources()
    {
        var response = await Client.GetAsync("/api/resources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }
}
