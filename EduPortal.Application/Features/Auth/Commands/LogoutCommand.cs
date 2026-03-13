using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cache;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokens, ITokenService tokenService, ICacheService cache)
    {
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _cache = cache;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hashed = _tokenService.HashToken(request.RefreshToken);
        var storedToken = await _refreshTokens.GetByTokenAsync(hashed, cancellationToken);

        if (storedToken != null && storedToken.IsActive)
        {
            storedToken.Revoke();
            var ttl = storedToken.ExpiresAt - DateTime.UtcNow;
            if (ttl > TimeSpan.Zero)
                await _cache.SetAsync($"revoked_token:{hashed}", true, ttl, cancellationToken);
            await _refreshTokens.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
