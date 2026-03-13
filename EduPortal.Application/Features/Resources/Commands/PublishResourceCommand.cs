using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Resources.Commands;

public record PublishResourceCommand(Guid Id) : IRequest<Result>;
public record UnpublishResourceCommand(Guid Id) : IRequest<Result>;

public class PublishResourceCommandHandler : IRequestHandler<PublishResourceCommand, Result>
{
    private readonly IResourceRepository _resources;
    public PublishResourceCommandHandler(IResourceRepository resources) => _resources = resources;

    public async Task<Result> Handle(PublishResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resources.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null) return Result.NotFound("Resource not found.");

        var hasContent = !string.IsNullOrEmpty(resource.FileKey) || !string.IsNullOrEmpty(resource.ExternalUrl) || !string.IsNullOrEmpty(resource.BlogContent);
        if (!hasContent) return Result.Failure("Resource must have content (FileKey, ExternalUrl, or BlogContent) before publishing.", 422);

        resource.Status = ResourceStatus.Published;
        await _resources.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UnpublishResourceCommandHandler : IRequestHandler<UnpublishResourceCommand, Result>
{
    private readonly IResourceRepository _resources;
    public UnpublishResourceCommandHandler(IResourceRepository resources) => _resources = resources;

    public async Task<Result> Handle(UnpublishResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resources.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null) return Result.NotFound("Resource not found.");
        resource.Status = ResourceStatus.Unpublished;
        await _resources.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
