using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Resources.Commands;

public record DeleteResourceCommand(Guid Id) : IRequest<Result>;

public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand, Result>
{
    private readonly IResourceRepository _resources;

    public DeleteResourceCommandHandler(IResourceRepository resources) => _resources = resources;

    public async Task<Result> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resources.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null) return Result.NotFound("Resource not found.");
        if (resource.Status == ResourceStatus.Published)
            return Result.Failure("Cannot delete a published resource. Unpublish it first.", 409);

        resource.IsDeleted = true;
        await _resources.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
