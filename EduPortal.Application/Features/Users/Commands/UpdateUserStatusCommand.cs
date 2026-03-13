using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Users.Commands;

public record UpdateUserStatusCommand(Guid UserId, bool IsActive) : IRequest<Result>;

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, Result>
{
    private readonly IUserRepository _users;

    public UpdateUserStatusCommandHandler(IUserRepository users) => _users = users;

    public async Task<Result> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return Result.NotFound("User not found.");
        user.IsActive = request.IsActive;
        await _users.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
