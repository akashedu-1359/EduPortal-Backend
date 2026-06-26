using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DomainCms = EduPortal.Domain.Entities.Cms;
using EduPortal.Infrastructure.Persistence;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.IntegrationTests.Admin;

[Trait("Category", "Integration")]
public class AdminCmsTests : IntegrationTestBase
{
    public AdminCmsTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Banners

    [Fact]
    public async Task GetBanners_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/banners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateBanner_WithAdmin_Returns201()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PostAsJsonAsync("/api/admin/cms/banners", new
        {
            Type = "Hero",
            Title = "Test Banner",
            Subtitle = "Sub title",
            ImageUrl = "https://example.com/img.jpg",
            CtaText = "Click Here",
            CtaUrl = "/resources",
            IsActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateBanner_NotFound_Returns404()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PutAsJsonAsync($"/api/admin/cms/banners/{Guid.NewGuid()}", new
        {
            Type = "Hero",
            Title = "Updated",
            IsActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBanner_NotFound_Returns404()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.DeleteAsync($"/api/admin/cms/banners/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BannerCrud_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/admin/cms/banners");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Pages

    [Fact]
    public async Task GetPages_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/pages");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetPageBySlug_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        // "about-us" was seeded by DataSeeder
        var response = await Client.GetAsync("/api/admin/cms/pages/about-us");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPageBySlug_NotFound_Returns404()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/pages/nonexistent-page");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePage_WithAdmin_ReturnsSuccess()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PutAsJsonAsync("/api/admin/cms/pages/about-us", new
        {
            Title = "About Us Updated",
            Content = "<p>Updated content</p>",
            MetaTitle = "About",
            MetaDescription = "About our portal",
            IsPublished = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region FAQs

    [Fact]
    public async Task GetFaqs_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/faqs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateFaq_WithAdmin_Returns201()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PostAsJsonAsync("/api/admin/cms/faqs", new
        {
            Question = "What is EduPortal?",
            Answer = "An education platform.",
            SortOrder = 1,
            IsVisible = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task DeleteFaq_NotFound_ReturnsError()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.DeleteAsync($"/api/admin/cms/faqs/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Footer

    [Fact]
    public async Task GetFooter_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/footer");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Sections

    [Fact]
    public async Task GetSections_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/sections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateSection_WithAdmin_ReturnsSuccess()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PatchAsJsonAsync("/api/admin/cms/sections/show_hero", new
        {
            IsVisible = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Settings

    [Fact]
    public async Task GetSettings_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateSetting_WithAdmin_ReturnsSuccess()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PutAsJsonAsync("/api/admin/cms/settings/site_name", new
        {
            Value = "EduPortal Updated"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Feature Flags

    [Fact]
    public async Task GetFeatureFlags_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/features");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ToggleFeatureFlag_WithAdmin_ReturnsSuccess()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PatchAsJsonAsync("/api/admin/cms/features/enable_payments", new
        {
            IsEnabled = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Promo Banners

    [Fact]
    public async Task GetPromoBanners_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/cms/promo-banners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreatePromoBanner_WithAdmin_Returns201()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PostAsJsonAsync("/api/admin/cms/promo-banners", new
        {
            ImageKey = "promo/img.jpg",
            Title = "Summer Sale",
            Link = "/resources",
            IsActive = true,
            SortOrder = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion
}
