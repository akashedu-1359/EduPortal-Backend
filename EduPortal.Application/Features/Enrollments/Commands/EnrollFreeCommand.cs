using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Enrollments.Commands;

public record EnrollFreeCommand(Guid ResourceId) : IRequest<Result>;

public class EnrollFreeCommandHandler : IRequestHandler<EnrollFreeCommand, Result>
{
    private readonly IResourceRepository _resources;
    private readonly IEnrollmentRepository _enrollments;
    private readonly ICurrentUserService _currentUser;

    public EnrollFreeCommandHandler(IResourceRepository resources, IEnrollmentRepository enrollments, ICurrentUserService currentUser)
    { _resources = resources; _enrollments = enrollments; _currentUser = currentUser; }

    public async Task<Result> Handle(EnrollFreeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var resource = await _resources.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource == null) return Result.NotFound("Resource not found.");
        if (resource.Status != ResourceStatus.Published) return Result.Failure("Resource is not available.", 400);
        if (resource.Price > 0) return Result.Failure("This resource requires purchase.", 400);

        var alreadyEnrolled = await _enrollments.IsEnrolledAsync(userId, request.ResourceId, cancellationToken);
        if (alreadyEnrolled) return Result.Success();

        var enrollment = Enrollment.Create(userId, request.ResourceId);
        await _enrollments.AddAsync(enrollment, cancellationToken);
        await _enrollments.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
