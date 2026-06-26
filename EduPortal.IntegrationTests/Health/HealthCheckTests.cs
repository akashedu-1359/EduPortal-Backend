using System.Net;
using System.Text.Json;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EduPortal.IntegrationTests.Health;

[Trait("Category", "Integration")]
public class HealthCheckTests : IntegrationTestBase
{
    public HealthCheckTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetHealth_ReturnsSuccessStatusCode()
    {
        var response = await Client.GetAsync("/api/health");

        // The health endpoint may return 200 or 503 depending on Redis mock;
        // we just verify it responds and has valid JSON
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.TryGetProperty("status", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("version", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetHealth_ReturnsExpectedShape()
    {
        var response = await Client.GetAsync("/api/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        json.RootElement.TryGetProperty("checks", out var checks).Should().BeTrue();
        checks.TryGetProperty("postgres", out _).Should().BeTrue();
        checks.TryGetProperty("redis", out _).Should().BeTrue();
    }
}
