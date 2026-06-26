using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EduPortal.Domain.Enums;
using DomainEntities = EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Persistence;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.IntegrationTests.User;

[Trait("Category", "Integration")]
public class UserEnrollmentTests : IntegrationTestBase
{
    public UserEnrollmentTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<Guid> SeedFreeResourceAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var admin = DomainEntities.User.CreateLocal("enrolladmin@test.com", "hash", "Enroll Admin");
        db.Users.Add(admin);

        var category = new DomainEntities.Category("Enroll Category", "enroll-category");
        db.Categories.Add(category);

        var resource = new DomainEntities.Resource("Free Resource", "A free resource", ResourceType.Blog, 0m, category.Id, admin.Id)
        {
            Status = ResourceStatus.Published,
            BlogContent = "Content here"
        };
        db.Resources.Add(resource);
        await db.SaveChangesAsync();

        return resource.Id;
    }

    [Fact]
    public async Task EnrollFree_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.PostAsync($"/api/user/resources/{Guid.NewGuid()}/enroll", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EnrollFree_WithAuth_ReturnsSuccess()
    {
        var resourceId = await SeedFreeResourceAsync();
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.PostAsync($"/api/user/resources/{resourceId}/enroll", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EnrollFree_NonexistentResource_ReturnsError()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.PostAsync($"/api/user/resources/{Guid.NewGuid()}/enroll", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAccess_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync($"/api/user/resources/{Guid.NewGuid()}/access");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAccess_WithAuth_ForNonexistentResource_ReturnsError()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync($"/api/user/resources/{Guid.NewGuid()}/access");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
    }
}
