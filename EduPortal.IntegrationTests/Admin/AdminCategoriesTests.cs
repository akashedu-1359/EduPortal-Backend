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
public class AdminCategoriesTests : IntegrationTestBase
{
    public AdminCategoriesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetCategories_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/admin/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategories_WithUserRole_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/admin/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCategories_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateCategory_WithAdmin_Returns201()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PostAsJsonAsync("/api/admin/categories", new
        {
            Name = "New Category",
            Description = "A new category",
            IsVisible = true,
            SortOrder = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").GetProperty("name").GetString().Should().Be("New Category");
    }

    [Fact]
    public async Task CreateCategory_DuplicateSlug_Returns409()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        await Client.PostAsJsonAsync("/api/admin/categories", new
        {
            Name = "Duplicate Cat",
            IsVisible = true,
            SortOrder = 1
        });

        var response = await Client.PostAsJsonAsync("/api/admin/categories", new
        {
            Name = "Duplicate Cat",
            IsVisible = true,
            SortOrder = 2
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateCategory_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        Guid categoryId;
        using (var scope = CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = new DomainEntities.Category("Update Me", "update-me");
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            categoryId = category.Id;
        }

        var response = await Client.PutAsJsonAsync($"/api/admin/categories/{categoryId}", new
        {
            Name = "Updated Category",
            Description = "Updated desc",
            IsVisible = true,
            SortOrder = 5
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCategory_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        Guid categoryId;
        using (var scope = CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = new DomainEntities.Category("Delete Me", "delete-me");
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            categoryId = category.Id;
        }

        var response = await Client.DeleteAsync($"/api/admin/categories/{categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
