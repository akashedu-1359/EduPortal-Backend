using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Cms.Commands;

public record UpdatePageCommand(string Slug, string Title, string Content, string? MetaTitle, string? MetaDescription, bool IsPublished) : IRequest<Result>;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, Result>
{
    private readonly ICmsRepository _cms;
    private readonly ICacheService _cache;
    private readonly IRevalidationService _revalidation;

    public UpdatePageCommandHandler(ICmsRepository cms, ICacheService cache, IRevalidationService revalidation)
    { _cms = cms; _cache = cache; _revalidation = revalidation; }

    public async Task<Result> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var page = await _cms.GetPageBySlugAsync(request.Slug, cancellationToken);
        if (page == null) return Result.NotFound("Page not found.");

        page.Title = request.Title; page.Content = request.Content;
        page.MetaTitle = request.MetaTitle; page.MetaDescription = request.MetaDescription;
        page.IsPublished = request.IsPublished; page.UpdatedAt = DateTime.UtcNow;

        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync($"cms:page:{request.Slug}", cancellationToken);
        _ = _revalidation.TriggerRevalidationAsync("cms-static-pages");
        return Result.Success();
    }
}
