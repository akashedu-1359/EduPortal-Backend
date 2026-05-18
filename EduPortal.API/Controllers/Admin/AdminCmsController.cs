using EduPortal.Application.Features.Cms.Commands;
using EduPortal.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/cms")]
[Authorize(Policy = "CmsManage")]
public class AdminCmsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICmsRepository _cms;
    private readonly IRevalidationService _revalidation;

    public AdminCmsController(IMediator mediator, ICmsRepository cms, IRevalidationService revalidation)
    { _mediator = mediator; _cms = cms; _revalidation = revalidation; }

    // Banners
    [HttpGet("banners")]
    public async Task<IActionResult> GetBanners(CancellationToken ct)
    {
        var banners = await _cms.GetBannersAsync(ct);
        return Ok(new { success = true, data = banners.Select(ToBannerDto) });
    }

    [HttpPost("banners")]
    public async Task<IActionResult> CreateBanner([FromBody] BannerRequest req, CancellationToken ct)
    {
        var banner = new EduPortal.Domain.Entities.Cms.CmsBanner
        {
            Key = Guid.NewGuid().ToString(),
            Type = req.Type,
            ImageKey = req.ImageUrl ?? "",
            Headline = req.Title,
            Subheadline = req.Subtitle,
            CtaText = req.CtaText,
            CtaLink = req.CtaUrl,
            IsActive = req.IsActive,
        };
        await _cms.AddBannerAsync(banner, ct);
        await _cms.SaveChangesAsync(ct);
        _ = _revalidation.TriggerRevalidationAsync("cms-homepage", ct);
        return StatusCode(201, new { success = true, data = ToBannerDto(banner) });
    }

    [HttpPut("banners/{id:guid}")]
    public async Task<IActionResult> UpdateBanner(Guid id, [FromBody] BannerRequest req, CancellationToken ct)
    {
        var banner = await _cms.GetBannerByIdAsync(id, ct);
        if (banner == null) return NotFound(new { success = false, error = "Banner not found." });
        banner.Type = req.Type;
        banner.ImageKey = req.ImageUrl ?? banner.ImageKey;
        banner.Headline = req.Title;
        banner.Subheadline = req.Subtitle;
        banner.CtaText = req.CtaText;
        banner.CtaLink = req.CtaUrl;
        banner.IsActive = req.IsActive;
        banner.UpdatedAt = DateTime.UtcNow;
        await _cms.SaveChangesAsync(ct);
        _ = _revalidation.TriggerRevalidationAsync("cms-homepage", ct);
        return Ok(new { success = true, data = ToBannerDto(banner) });
    }

    [HttpDelete("banners/{id:guid}")]
    public async Task<IActionResult> DeleteBanner(Guid id, CancellationToken ct)
    {
        var banner = await _cms.GetBannerByIdAsync(id, ct);
        if (banner == null) return NotFound(new { success = false, error = "Banner not found." });
        _cms.RemoveBanner(banner);
        await _cms.SaveChangesAsync(ct);
        _ = _revalidation.TriggerRevalidationAsync("cms-homepage", ct);
        return Ok(new { success = true });
    }

    private static object ToBannerDto(EduPortal.Domain.Entities.Cms.CmsBanner b) => new
    {
        b.Id, b.Type, title = b.Headline, subtitle = b.Subheadline,
        imageUrl = b.ImageKey, ctaText = b.CtaText, ctaUrl = b.CtaLink, b.IsActive
    };

    // Pages
    [HttpGet("pages")]
    public async Task<IActionResult> GetPages(CancellationToken ct) =>
        Ok(new { success = true, data = (await _cms.GetPagesAsync(ct)).Select(p => new { p.Id, p.Slug, p.Title, p.IsPublished, p.UpdatedAt }) });

    [HttpGet("pages/{slug}")]
    public async Task<IActionResult> GetPage(string slug, CancellationToken ct)
    {
        var page = await _cms.GetPageBySlugAsync(slug, ct);
        return page == null ? NotFound(new { success = false, error = "Page not found." })
            : Ok(new { success = true, data = page });
    }

    [HttpPut("pages/{slug}")]
    public async Task<IActionResult> UpdatePage(string slug, [FromBody] UpdatePageRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdatePageCommand(slug, req.Title, req.Content, req.MetaTitle, req.MetaDescription, req.IsPublished), ct);
        return result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    // FAQs
    [HttpGet("faqs")]
    public async Task<IActionResult> GetFaqs(CancellationToken ct) =>
        Ok(new { success = true, data = await _cms.GetFaqsAsync(ct) });

    [HttpPost("faqs")]
    public async Task<IActionResult> CreateFaq([FromBody] CreateFaqCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return result.IsSuccess ? StatusCode(201, new { success = true, data = result.Value }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpPut("faqs/{id:guid}")]
    public async Task<IActionResult> UpdateFaq(Guid id, [FromBody] UpdateFaqRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateFaqCommand(id, req.Question, req.Answer, req.SortOrder, req.IsVisible), ct);
        return result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpDelete("faqs/{id:guid}")]
    public async Task<IActionResult> DeleteFaq(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteFaqCommand(id), ct);
        return result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpPatch("faqs/reorder")]
    public async Task<IActionResult> ReorderFaqs([FromBody] ReorderFaqsCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    // Footer
    [HttpGet("footer")]
    public async Task<IActionResult> GetFooter(CancellationToken ct) =>
        Ok(new { success = true, data = await _cms.GetFooterAsync(ct) });

    [HttpPut("footer")]
    public async Task<IActionResult> UpdateFooter([FromBody] UpdateFooterCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    // Sections
    [HttpGet("sections")]
    public async Task<IActionResult> GetSections(CancellationToken ct) =>
        Ok(new { success = true, data = await _cms.GetSectionsAsync(ct) });

    [HttpPatch("sections/{key}")]
    public async Task<IActionResult> UpdateSection(string key, [FromBody] UpdateSectionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateSectionVisibilityCommand(key, req.IsVisible), ct);
        return result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    // Settings
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct) =>
        Ok(new { success = true, data = await _cms.GetSettingsAsync(ct) });

    [HttpPut("settings/{key}")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateSettingCommand(key, req.Value), ct);
        return result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    // Feature Flags
    [HttpGet("features")]
    public async Task<IActionResult> GetFeatureFlags(CancellationToken ct) =>
        Ok(new { success = true, data = await _cms.GetFeatureFlagsAsync(ct) });

    [HttpPatch("features/{key}")]
    public async Task<IActionResult> ToggleFlag(string key, [FromBody] ToggleFlagRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleFeatureFlagCommand(key, req.IsEnabled), ct);
        return result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    // Promo Banners
    [HttpGet("promo-banners")]
    public async Task<IActionResult> GetPromoBanners(CancellationToken ct) =>
        Ok(new { success = true, data = await _cms.GetPromoBannersAsync(ct) });

    [HttpGet("promo-banners/active")]
    public async Task<IActionResult> GetActivePromoBanners(CancellationToken ct) =>
        Ok(new { success = true, data = await _cms.GetActivePromoBannersAsync(ct) });

    [HttpPost("promo-banners")]
    public async Task<IActionResult> CreatePromoBanner([FromBody] CreatePromoBannerRequest req, CancellationToken ct)
    {
        var banner = new EduPortal.Domain.Entities.Cms.CmsPromoBanner
        {
            ImageKey = req.ImageKey, Title = req.Title, Link = req.Link,
            StartDate = req.StartDate, EndDate = req.EndDate, IsActive = req.IsActive, SortOrder = req.SortOrder
        };
        await _cms.AddPromoBannerAsync(banner, ct);
        await _cms.SaveChangesAsync(ct);
        return StatusCode(201, new { success = true, data = banner });
    }
}

public record BannerRequest(string Type, string Title, string? Subtitle, string? ImageUrl, string? CtaText, string? CtaUrl, bool IsActive);
public record UpdatePageRequest(string Title, string Content, string? MetaTitle, string? MetaDescription, bool IsPublished);
public record UpdateFaqRequest(string Question, string Answer, int SortOrder, bool IsVisible);
public record UpdateSectionRequest(bool IsVisible);
public record UpdateSettingRequest(string Value);
public record ToggleFlagRequest(bool IsEnabled);
public record CreatePromoBannerRequest(string ImageKey, string? Title, string? Link, DateOnly? StartDate, DateOnly? EndDate, bool IsActive, int SortOrder);
