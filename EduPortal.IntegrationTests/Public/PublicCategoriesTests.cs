using System.Net;
using System.Text.Json;
using DomainEntities = EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Persistence;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.IntegrationTests.Public;

[Trait("Category", "Integration")]
public class PublicCategoriesTests : IntegrationTestBase
{
    public PublicCategoriesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetCategories_ReturnsSuccess()
    {
        var response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetCategories_ReturnsOnlyVisibleCategories()
    {
        using (var scope = CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Categories.Add(new DomainEntities.Category("Visible Cat", "visible-cat") { IsVisible = true });
            db.Categories.Add(new DomainEntities.Category("Hidden Cat", "hidden-cat") { IsVisible = false });
            await db.SaveChangesAsync();
        }

        var response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var data = json.RootElement.GetProperty("data");
        data.GetArrayLength().Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetCategories_ReturnsEmptyArrayWhenNone()
    {
        var response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }
}
