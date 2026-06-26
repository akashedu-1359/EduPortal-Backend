using System.Net;
using System.Text.Json;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EduPortal.IntegrationTests.Admin;

[Trait("Category", "Integration")]
public class AdminAnalyticsTests : IntegrationTestBase
{
    public AdminAnalyticsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetSummary_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/admin/analytics/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSummary_WithUserRole_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/admin/analytics/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSummary_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/analytics/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetRevenue_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/analytics/revenue?days=30");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetRevenue_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/admin/analytics/revenue");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEnrollments_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/analytics/enrollments?days=30");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetEnrollments_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/admin/analytics/enrollments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
