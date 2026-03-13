using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using MediatR;

namespace EduPortal.Application.Features.Auth.Commands;

public record RegisterCommand(string Email, string Password, string FullName) : IRequest<Result<AuthResponse>>;

public record AuthResponse(string AccessToken, UserDto User);
public record UserDto(Guid Id, string Email, string FullName, string Role);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository users, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _users = users;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            return Result<AuthResponse>.Conflict("Email is already registered.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.CreateLocal(request.Email, passwordHash, request.FullName);

        await _users.AddAsync(user, cancellationToken);
        await _users.AssignRoleAsync(user.Id, RoleConstants.UserId, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        var permissions = await _users.GetPermissionsAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, RoleConstants.User, permissions);

        return Result<AuthResponse>.Created(new AuthResponse(accessToken, new UserDto(user.Id, user.Email, user.FullName, RoleConstants.User)));
    }
}
