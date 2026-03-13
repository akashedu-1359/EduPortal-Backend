using EduPortal.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/auth/google")]
public class GoogleAuthController : ControllerBase
{
    private readonly GoogleAuthService _googleAuth;
    private readonly string _frontendUrl;

    public GoogleAuthController(GoogleAuthService googleAuth, IConfiguration config)
    {
        _googleAuth = googleAuth;
        _frontendUrl = config["Frontend:BaseUrl"] ?? "http://localhost:3000";
    }

    [HttpGet("redirect")]
    public IActionResult Redirect()
    {
        var url = _googleAuth.BuildAuthorizationUrl();
        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string? error, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            return Redirect($"{_frontendUrl}/auth/callback?error=oauth_failed");

        var result = await _googleAuth.HandleCallbackAsync(code, ct);
        if (result == null)
            return Redirect($"{_frontendUrl}/auth/callback?error=oauth_failed");

        var (accessToken, refreshToken, _) = result.Value;

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return Redirect($"{_frontendUrl}/auth/callback#access_token={Uri.EscapeDataString(accessToken)}");
    }
}
