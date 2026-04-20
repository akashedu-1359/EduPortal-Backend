using EduPortal.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? StatusCode(201, new { success = true, data = result.Value })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, error = result.Error });

        var cookieExpiry = DateTimeOffset.UtcNow.AddDays(30);

        Response.Cookies.Append("refresh_token", result.Value!.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = cookieExpiry
        });

        // Non-httpOnly so Next.js middleware can use it for role-based routing
        Response.Cookies.Append("eduportal_role", result.Value.User.Role, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = cookieExpiry
        });

        // refreshToken is included so the Next.js route handler can set it as an httpOnly cookie.
        // The route handler strips it before forwarding the response to the browser.
        return Ok(new { success = true, data = new { result.Value.AccessToken, result.Value.User, result.Value.RefreshToken } });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { success = false, error = "No refresh token." });

        var result = await _mediator.Send(new RefreshTokenCommand(refreshToken), ct);
        if (!result.IsSuccess)
        {
            Response.Cookies.Delete("refresh_token");
            return Unauthorized(new { success = false, error = result.Error });
        }

        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(30);

        Response.Cookies.Append("refresh_token", result.Value!.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshExpiry
        });

        Response.Cookies.Append("eduportal_role", result.Value.User.Role, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshExpiry
        });

        return Ok(new { success = true, data = new { result.Value.AccessToken, result.Value.User, result.Value.RefreshToken } });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refresh_token"] ?? "";
        await _mediator.Send(new LogoutCommand(refreshToken), ct);
        Response.Cookies.Delete("refresh_token");
        Response.Cookies.Delete("eduportal_role");
        return Ok(new { success = true });
    }
}
