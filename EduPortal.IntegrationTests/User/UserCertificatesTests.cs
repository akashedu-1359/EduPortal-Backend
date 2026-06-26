using System.Net;
using System.Text.Json;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EduPortal.IntegrationTests.User;

[Trait("Category", "Integration")]
public class UserCertificatesTests : IntegrationTestBase
{
    public UserCertificatesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetCertificates_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/user/certificates");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCertificates_WithAuth_Returns200()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/user/certificates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DownloadCertificate_WhenNotFound_ReturnsError()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync($"/api/user/certificates/{Guid.NewGuid()}/download");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DownloadCertificate_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync($"/api/user/certificates/{Guid.NewGuid()}/download");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
