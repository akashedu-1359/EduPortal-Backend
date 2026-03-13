using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence.Repositories;

public class CmsRepository : ICmsRepository
{
    private readonly AppDbContext _db;
    public CmsRepository(AppDbContext db) => _db = db;

    public Task<List<CmsBanner>> GetBannersAsync(CancellationToken ct) => _db.CmsBanners.ToListAsync(ct);
    public Task<CmsBanner?> GetBannerByKeyAsync(string key, CancellationToken ct) => _db.CmsBanners.FirstOrDefaultAsync(b => b.Key == key, ct);
    public async Task AddBannerAsync(CmsBanner banner, CancellationToken ct) => await _db.CmsBanners.AddAsync(banner, ct);

    public Task<List<CmsPage>> GetPagesAsync(CancellationToken ct) => _db.CmsPages.ToListAsync(ct);
    public Task<CmsPage?> GetPageBySlugAsync(string slug, CancellationToken ct) => _db.CmsPages.FirstOrDefaultAsync(p => p.Slug == slug, ct);
    public async Task AddPageAsync(CmsPage page, CancellationToken ct) => await _db.CmsPages.AddAsync(page, ct);

    public Task<List<CmsFaq>> GetFaqsAsync(CancellationToken ct) => _db.CmsFaqs.OrderBy(f => f.SortOrder).ToListAsync(ct);
    public async Task AddFaqAsync(CmsFaq faq, CancellationToken ct) => await _db.CmsFaqs.AddAsync(faq, ct);
    public void RemoveFaq(CmsFaq faq) => _db.CmsFaqs.Remove(faq);

    public Task<CmsFooter?> GetFooterAsync(CancellationToken ct) => _db.CmsFooters.FirstOrDefaultAsync(ct);

    public Task<List<CmsSection>> GetSectionsAsync(CancellationToken ct) => _db.CmsSections.ToListAsync(ct);

    public Task<List<CmsSetting>> GetSettingsAsync(CancellationToken ct) => _db.CmsSettings.ToListAsync(ct);
    public Task<CmsSetting?> GetSettingByKeyAsync(string key, CancellationToken ct) => _db.CmsSettings.FindAsync(new object[] { key }, ct).AsTask();

    public Task<List<CmsFeatureFlag>> GetFeatureFlagsAsync(CancellationToken ct) => _db.CmsFeatureFlags.ToListAsync(ct);

    public Task<List<CmsPromoBanner>> GetPromoBannersAsync(CancellationToken ct) => _db.CmsPromoBanners.OrderBy(p => p.SortOrder).ToListAsync(ct);
    public Task<List<CmsPromoBanner>> GetActivePromoBannersAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _db.CmsPromoBanners.Where(p => p.IsActive && (p.StartDate == null || p.StartDate <= today) && (p.EndDate == null || p.EndDate >= today)).OrderBy(p => p.SortOrder).ToListAsync(ct);
    }
    public async Task AddPromoBannerAsync(CmsPromoBanner banner, CancellationToken ct) => await _db.CmsPromoBanners.AddAsync(banner, ct);
    public void RemovePromoBanner(CmsPromoBanner banner) => _db.CmsPromoBanners.Remove(banner);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
