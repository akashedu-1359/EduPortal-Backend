using EduPortal.Domain.Entities.Cms;

namespace EduPortal.Application.Interfaces;

public interface ICmsRepository
{
    // Banners
    Task<List<CmsBanner>> GetBannersAsync(CancellationToken ct = default);
    Task<CmsBanner?> GetBannerByKeyAsync(string key, CancellationToken ct = default);
    Task AddBannerAsync(CmsBanner banner, CancellationToken ct = default);

    // Pages
    Task<List<CmsPage>> GetPagesAsync(CancellationToken ct = default);
    Task<CmsPage?> GetPageBySlugAsync(string slug, CancellationToken ct = default);
    Task AddPageAsync(CmsPage page, CancellationToken ct = default);

    // FAQs
    Task<List<CmsFaq>> GetFaqsAsync(CancellationToken ct = default);
    Task AddFaqAsync(CmsFaq faq, CancellationToken ct = default);

    // Footer
    Task<CmsFooter?> GetFooterAsync(CancellationToken ct = default);

    // Sections
    Task<List<CmsSection>> GetSectionsAsync(CancellationToken ct = default);

    // Settings
    Task<List<CmsSetting>> GetSettingsAsync(CancellationToken ct = default);
    Task<CmsSetting?> GetSettingByKeyAsync(string key, CancellationToken ct = default);

    // Feature Flags
    Task<List<CmsFeatureFlag>> GetFeatureFlagsAsync(CancellationToken ct = default);

    // Promo Banners
    Task<List<CmsPromoBanner>> GetPromoBannersAsync(CancellationToken ct = default);
    Task AddPromoBannerAsync(CmsPromoBanner banner, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
