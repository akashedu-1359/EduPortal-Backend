using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using MediatR;

namespace EduPortal.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(string AccessToken, string RefreshToken, UserDto User);

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IUserRepository users, IRefreshTokenRepository refreshTokens, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || user.PasswordHash == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid email or password.", 401);

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Account is deactivated.", 403);

        var role = await _users.GetRoleNameAsync(user.Id, cancellationToken) ?? RoleConstants.User;
        var permissions = await _users.GetPermissionsAsync(user.Id, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, role, permissions);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var hashedToken = _tokenService.HashToken(rawRefreshToken);

        var refreshToken = RefreshToken.Create(user.Id, hashedToken, DateTime.UtcNow.AddDays(30));
        await _refreshTokens.AddAsync(refreshToken, cancellationToken);
        await _refreshTokens.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse(accessToken, rawRefreshToken, new UserDto(user.Id, user.Email, user.FullName, role)));
    }
}
