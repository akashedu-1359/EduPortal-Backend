using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Public;

[ApiController]
[Route("api/cms")]
public class PublicCmsController : ControllerBase
{
    private readonly ICmsRepository _cms;
    private readonly ICacheService _cache;

    public PublicCmsController(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    [HttpGet("banner/{key}")]
    public async Task<IActionResult> GetBanner(string key, CancellationToken ct)
    {
        var cacheKey = $"cms:banner:{key}";
        var cached = await _cache.GetAsync<object>(cacheKey, ct);
        if (cached != null) return Ok(new { success = true, data = cached });

        var banner = await _cms.GetBannerByKeyAsync(key, ct);
        if (banner == null) return NotFound(new { success = false, error = "Banner not found." });

        await _cache.SetAsync(cacheKey, banner, TimeSpan.FromHours(1), ct);
        return Ok(new { success = true, data = banner });
    }

    [HttpGet("page/{slug}")]
    public async Task<IActionResult> GetPage(string slug, CancellationToken ct)
    {
        var cacheKey = $"cms:page:{slug}";
        var cached = await _cache.GetAsync<object>(cacheKey, ct);
        if (cached != null) return Ok(new { success = true, data = cached });

        var page = await _cms.GetPageBySlugAsync(slug, ct);
        if (page == null || !page.IsPublished) return NotFound(new { success = false, error = "Page not found." });

        var dto = new { page.Slug, page.Title, page.Content, page.MetaTitle, page.MetaDescription };
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromHours(1), ct);
        return Ok(new { success = true, data = dto });
    }

    [HttpGet("faqs")]
    public async Task<IActionResult> GetFaqs(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<object>("cms:faqs", ct);
        if (cached != null) return Ok(new { success = true, data = cached });

        var faqs = (await _cms.GetFaqsAsync(ct)).Where(f => f.IsVisible)
            .Select(f => new { f.Id, f.Question, f.Answer, f.SortOrder }).ToList();
        await _cache.SetAsync("cms:faqs", faqs, TimeSpan.FromHours(1), ct);
        return Ok(new { success = true, data = faqs });
    }

    [HttpGet("footer")]
    public async Task<IActionResult> GetFooter(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<object>("cms:footer", ct);
        if (cached != null) return Ok(new { success = true, data = cached });

        var footer = await _cms.GetFooterAsync(ct);
        await _cache.SetAsync("cms:footer", footer, TimeSpan.FromHours(1), ct);
        return Ok(new { success = true, data = footer });
    }

    [HttpGet("sections")]
    public async Task<IActionResult> GetSections(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<object>("cms:sections", ct);
        if (cached != null) return Ok(new { success = true, data = cached });

        var sections = (await _cms.GetSectionsAsync(ct)).Select(s => new { s.Key, s.IsVisible, s.Label }).ToList();
        await _cache.SetAsync("cms:sections", sections, TimeSpan.FromHours(1), ct);
        return Ok(new { success = true, data = sections });
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<object>("cms:settings", ct);
        if (cached != null) return Ok(new { success = true, data = cached });

        var settings = (await _cms.GetSettingsAsync(ct)).Select(s => new { s.Key, s.Value, s.DataType }).ToList();
        await _cache.SetAsync("cms:settings", settings, TimeSpan.FromHours(1), ct);
        return Ok(new { success = true, data = settings });
    }

    [HttpGet("features")]
    public async Task<IActionResult> GetFeatures(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<object>("cms:features", ct);
        if (cached != null) return Ok(new { success = true, data = cached });

        var flags = (await _cms.GetFeatureFlagsAsync(ct)).Select(f => new { f.Key, f.IsEnabled }).ToList();
        await _cache.SetAsync("cms:features", flags, TimeSpan.FromMinutes(30), ct);
        return Ok(new { success = true, data = flags });
    }

    [HttpGet("promo-banners/active")]
    public async Task<IActionResult> GetActivePromoBanners(CancellationToken ct)
    {
        var banners = await _cms.GetActivePromoBannersAsync(ct);
        return Ok(new { success = true, data = banners });
    }
}
