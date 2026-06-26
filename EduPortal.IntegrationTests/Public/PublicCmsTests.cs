using System.Net;
using System.Text.Json;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EduPortal.IntegrationTests.Public;

[Trait("Category", "Integration")]
public class PublicCmsTests : IntegrationTestBase
{
    public PublicCmsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetHomepage_ReturnsSuccessWithExpectedShape()
    {
        var response = await Client.GetAsync("/api/cms/homepage");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();

        var data = json.RootElement.GetProperty("data");
        data.TryGetProperty("banners", out _).Should().BeTrue();
        data.TryGetProperty("sections", out _).Should().BeTrue();
        data.TryGetProperty("settings", out _).Should().BeTrue();
        data.TryGetProperty("featureFlags", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetBannerByKey_WhenNotFound_Returns404()
    {
        var response = await Client.GetAsync("/api/cms/banner/nonexistent-key");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetPageBySlug_WhenNotPublished_Returns404()
    {
        // Seeded pages are all IsPublished = false
        var response = await Client.GetAsync("/api/cms/page/about-us");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPageBySlug_WhenNotFound_Returns404()
    {
        var response = await Client.GetAsync("/api/cms/page/nonexistent-page");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFaqs_ReturnsSuccess()
    {
        var response = await Client.GetAsync("/api/cms/faqs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetFooter_ReturnsSuccess()
    {
        var response = await Client.GetAsync("/api/cms/footer");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetSections_ReturnsSuccess()
    {
        var response = await Client.GetAsync("/api/cms/sections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetSettings_ReturnsSuccess()
    {
        var response = await Client.GetAsync("/api/cms/settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetFeatures_ReturnsSuccess()
    {
        var response = await Client.GetAsync("/api/cms/features");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetActivePromoBanners_ReturnsSuccess()
    {
        var response = await Client.GetAsync("/api/cms/promo-banners/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }
}
