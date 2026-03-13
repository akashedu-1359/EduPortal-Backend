using EduPortal.Domain.Entities.Cms;

namespace EduPortal.Application.Interfaces;

public interface ICmsRepository
{
    Task<List<CmsBanner>> GetBannersAsync(CancellationToken ct = default);
    Task<CmsBanner?> GetBannerByKeyAsync(string key, CancellationToken ct = default);
    Task AddBannerAsync(CmsBanner banner, CancellationToken ct = default);

    Task<List<CmsPage>> GetPagesAsync(CancellationToken ct = default);
    Task<CmsPage?> GetPageBySlugAsync(string slug, CancellationToken ct = default);
    Task AddPageAsync(CmsPage page, CancellationToken ct = default);

    Task<List<CmsFaq>> GetFaqsAsync(CancellationToken ct = default);
    Task AddFaqAsync(CmsFaq faq, CancellationToken ct = default);
    void RemoveFaq(CmsFaq faq);

    Task<CmsFooter?> GetFooterAsync(CancellationToken ct = default);

    Task<List<CmsSection>> GetSectionsAsync(CancellationToken ct = default);

    Task<List<CmsSetting>> GetSettingsAsync(CancellationToken ct = default);
    Task<CmsSetting?> GetSettingByKeyAsync(string key, CancellationToken ct = default);

    Task<List<CmsFeatureFlag>> GetFeatureFlagsAsync(CancellationToken ct = default);

    Task<List<CmsPromoBanner>> GetPromoBannersAsync(CancellationToken ct = default);
    Task<List<CmsPromoBanner>> GetActivePromoBannersAsync(CancellationToken ct = default);
    Task AddPromoBannerAsync(CmsPromoBanner banner, CancellationToken ct = default);
    void RemovePromoBanner(CmsPromoBanner banner);

    Task SaveChangesAsync(CancellationToken ct = default);
}
