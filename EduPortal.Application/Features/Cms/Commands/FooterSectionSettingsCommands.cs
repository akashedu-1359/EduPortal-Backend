using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Cms.Commands;

// Footer
public record UpdateFooterCommand(string CompanyText, string CopyrightText, string SocialLinksJson, string ColumnsJson) : IRequest<Result>;

public class UpdateFooterCommandHandler : IRequestHandler<UpdateFooterCommand, Result>
{
    private readonly ICmsRepository _cms; private readonly ICacheService _cache;
    public UpdateFooterCommandHandler(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    public async Task<Result> Handle(UpdateFooterCommand request, CancellationToken cancellationToken)
    {
        var footer = await _cms.GetFooterAsync(cancellationToken);
        if (footer == null) return Result.NotFound("Footer not found.");
        footer.CompanyText = request.CompanyText; footer.CopyrightText = request.CopyrightText;
        footer.SocialLinksJson = request.SocialLinksJson; footer.ColumnsJson = request.ColumnsJson;
        footer.UpdatedAt = DateTime.UtcNow;
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync("cms:footer", cancellationToken);
        return Result.Success();
    }
}

// Section visibility
public record UpdateSectionVisibilityCommand(string Key, bool IsVisible) : IRequest<Result>;

public class UpdateSectionVisibilityCommandHandler : IRequestHandler<UpdateSectionVisibilityCommand, Result>
{
    private readonly ICmsRepository _cms; private readonly ICacheService _cache;
    public UpdateSectionVisibilityCommandHandler(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    public async Task<Result> Handle(UpdateSectionVisibilityCommand request, CancellationToken cancellationToken)
    {
        var sections = await _cms.GetSectionsAsync(cancellationToken);
        var section = sections.FirstOrDefault(s => s.Key == request.Key);
        if (section == null) return Result.NotFound("Section not found.");
        section.IsVisible = request.IsVisible;
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync("cms:sections", cancellationToken);
        return Result.Success();
    }
}

// Setting update
public record UpdateSettingCommand(string Key, string Value) : IRequest<Result>;

public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand, Result>
{
    private readonly ICmsRepository _cms; private readonly ICacheService _cache;
    public UpdateSettingCommandHandler(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    public async Task<Result> Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _cms.GetSettingByKeyAsync(request.Key, cancellationToken);
        if (setting == null) return Result.NotFound("Setting not found.");
        setting.Value = request.Value; setting.UpdatedAt = DateTime.UtcNow;
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync("cms:settings", cancellationToken);
        return Result.Success();
    }
}

// Feature flag toggle
public record ToggleFeatureFlagCommand(string Key, bool IsEnabled) : IRequest<Result>;

public class ToggleFeatureFlagCommandHandler : IRequestHandler<ToggleFeatureFlagCommand, Result>
{
    private readonly ICmsRepository _cms; private readonly ICacheService _cache;
    public ToggleFeatureFlagCommandHandler(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    public async Task<Result> Handle(ToggleFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flags = await _cms.GetFeatureFlagsAsync(cancellationToken);
        var flag = flags.FirstOrDefault(f => f.Key == request.Key);
        if (flag == null) return Result.NotFound("Feature flag not found.");
        flag.IsEnabled = request.IsEnabled; flag.UpdatedAt = DateTime.UtcNow;
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync("cms:features", cancellationToken);
        return Result.Success();
    }
}
