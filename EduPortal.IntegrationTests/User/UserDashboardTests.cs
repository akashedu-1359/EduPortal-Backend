using System.Net;
using System.Text.Json;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EduPortal.IntegrationTests.User;

[Trait("Category", "Integration")]
public class UserDashboardTests : IntegrationTestBase
{
    public UserDashboardTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetDashboard_WithoutToken_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/user/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDashboard_WithValidToken_Returns200()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/user/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDashboard_WithExpiredToken_Returns401()
    {
        ClearAuthentication();
        // No valid token set
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        var response = await Client.GetAsync("/api/user/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
