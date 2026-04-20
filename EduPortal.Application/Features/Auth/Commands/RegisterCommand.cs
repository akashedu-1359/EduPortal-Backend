using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using MediatR;

namespace EduPortal.Application.Features.Auth.Commands;

public record RegisterCommand(string Email, string Password, string FullName) : IRequest<Result<LoginResponse>>;

public record UserDto(Guid Id, string Email, string FullName, string Role);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository users, IRefreshTokenRepository refreshTokens, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<LoginResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            return Result<LoginResponse>.Conflict("Email is already registered.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.CreateLocal(request.Email, passwordHash, request.FullName);

        await _users.AddAsync(user, cancellationToken);
        await _users.AssignRoleAsync(user.Id, RoleConstants.UserId, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        var permissions = await _users.GetPermissionsAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, RoleConstants.User, permissions);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var hashedToken = _tokenService.HashToken(rawRefreshToken);

        var refreshToken = RefreshToken.Create(user.Id, hashedToken, DateTime.UtcNow.AddDays(30));
        await _refreshTokens.AddAsync(refreshToken, cancellationToken);
        await _refreshTokens.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse>.Created(new LoginResponse(accessToken, rawRefreshToken, new UserDto(user.Id, user.Email, user.FullName, RoleConstants.User)));
    }
}
