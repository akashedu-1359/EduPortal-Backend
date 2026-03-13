using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using MediatR;

namespace EduPortal.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<TokenRefreshResponse>>;
public record TokenRefreshResponse(string AccessToken, string RefreshToken);

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenRefreshResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cache;

    public RefreshTokenCommandHandler(IUserRepository users, IRefreshTokenRepository refreshTokens, ITokenService tokenService, ICacheService cache)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _cache = cache;
    }

    public async Task<Result<TokenRefreshResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashed = _tokenService.HashToken(request.RefreshToken);

        // Check Redis revocation list
        var revoked = await _cache.GetAsync<bool?>($"revoked_token:{hashed}", cancellationToken);
        if (revoked == true)
            return Result<TokenRefreshResponse>.Unauthorized("Token has been revoked.");

        var storedToken = await _refreshTokens.GetByTokenAsync(hashed, cancellationToken);
        if (storedToken == null || !storedToken.IsActive)
            return Result<TokenRefreshResponse>.Unauthorized("Invalid or expired refresh token.");

        var user = await _users.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            return Result<TokenRefreshResponse>.Unauthorized();

        var role = await _users.GetRoleNameAsync(user.Id, cancellationToken) ?? RoleConstants.User;
        var permissions = await _users.GetPermissionsAsync(user.Id, cancellationToken);

        // Rotate token
        storedToken.Revoke();
        var ttl = storedToken.ExpiresAt - DateTime.UtcNow;
        if (ttl > TimeSpan.Zero)
            await _cache.SetAsync($"revoked_token:{hashed}", true, ttl, cancellationToken);

        var newRawToken = _tokenService.GenerateRefreshToken();
        var newHashed = _tokenService.HashToken(newRawToken);
        var newRefreshToken = RefreshToken.Create(user.Id, newHashed, DateTime.UtcNow.AddDays(30));

        await _refreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokens.SaveChangesAsync(cancellationToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, role, permissions);
        return Result<TokenRefreshResponse>.Success(new TokenRefreshResponse(newAccessToken, newRawToken));
    }
}
