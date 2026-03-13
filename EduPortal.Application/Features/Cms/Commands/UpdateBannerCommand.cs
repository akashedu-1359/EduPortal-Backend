using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Cms.Commands;

public record UpdateBannerCommand(string Key, string ImageKey, string Headline, string? Subheadline, string? CtaText, string? CtaLink, bool IsActive) : IRequest<Result>;

public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, Result>
{
    private readonly ICmsRepository _cms;
    private readonly ICacheService _cache;
    private readonly IRevalidationService _revalidation;
    private readonly ICurrentUserService _currentUser;

    public UpdateBannerCommandHandler(ICmsRepository cms, ICacheService cache, IRevalidationService revalidation, ICurrentUserService currentUser)
    { _cms = cms; _cache = cache; _revalidation = revalidation; _currentUser = currentUser; }

    public async Task<Result> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await _cms.GetBannerByKeyAsync(request.Key, cancellationToken);
        if (banner == null)
        {
            await _cms.AddBannerAsync(new EduPortal.Domain.Entities.Cms.CmsBanner
            {
                Key = request.Key, ImageKey = request.ImageKey, Headline = request.Headline,
                Subheadline = request.Subheadline, CtaText = request.CtaText, CtaLink = request.CtaLink,
                IsActive = request.IsActive, UpdatedByAdminId = _currentUser.UserId ?? Guid.Empty
            }, cancellationToken);
        }
        else
        {
            banner.ImageKey = request.ImageKey; banner.Headline = request.Headline;
            banner.Subheadline = request.Subheadline; banner.CtaText = request.CtaText;
            banner.CtaLink = request.CtaLink; banner.IsActive = request.IsActive;
            banner.UpdatedByAdminId = _currentUser.UserId ?? Guid.Empty;
            banner.UpdatedAt = DateTime.UtcNow;
        }
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync($"cms:banner:{request.Key}", cancellationToken);
        _ = _revalidation.TriggerRevalidationAsync("cms-homepage");
        return Result.Success();
    }
}
