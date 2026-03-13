using System.Text.Json;
using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class GoogleAuthService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokenService;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly HttpClient _http;

    public GoogleAuthService(IConfiguration config, IUserRepository users, IRefreshTokenRepository refreshTokens, ITokenService tokenService, ILogger<GoogleAuthService> logger, IHttpClientFactory httpFactory)
    {
        _clientId = config["Google:ClientId"] ?? throw new InvalidOperationException("Google:ClientId not configured");
        _clientSecret = config["Google:ClientSecret"] ?? throw new InvalidOperationException("Google:ClientSecret not configured");
        _redirectUri = config["Google:RedirectUri"] ?? throw new InvalidOperationException("Google:RedirectUri not configured");
        _users = users;
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _logger = logger;
        _http = httpFactory.CreateClient();
    }

    public string BuildAuthorizationUrl()
    {
        var state = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return $"https://accounts.google.com/o/oauth2/v2/auth" +
               $"?client_id={Uri.EscapeDataString(_clientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
               $"&response_type=code" +
               $"&scope={Uri.EscapeDataString("openid email profile")}" +
               $"&state={state}" +
               $"&access_type=offline";
    }

    public async Task<(string AccessToken, string RefreshToken, GoogleUserInfo User)?> HandleCallbackAsync(string code, CancellationToken ct)
    {
        // Exchange code for tokens
        var tokenResponse = await _http.PostAsync("https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["redirect_uri"] = _redirectUri,
                ["grant_type"] = "authorization_code"
            }), ct);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Google token exchange failed: {Status}", tokenResponse.StatusCode);
            return null;
        }

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(ct);
        var tokenData = JsonDocument.Parse(tokenJson);
        var idToken = tokenData.RootElement.GetProperty("id_token").GetString();

        // Get user info from Google
        var userInfoResponse = await _http.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo", ct);
        if (!userInfoResponse.IsSuccessStatusCode) return null;

        var userJson = await userInfoResponse.Content.ReadAsStringAsync(ct);
        var userDoc = JsonDocument.Parse(userJson);
        var root = userDoc.RootElement;

        var googleUser = new GoogleUserInfo(
            root.GetProperty("sub").GetString()!,
            root.GetProperty("email").GetString()!,
            root.TryGetProperty("name", out var name) ? name.GetString()! : "User",
            root.TryGetProperty("picture", out var pic) ? pic.GetString() : null
        );

        // Upsert user
        var user = await _users.GetByEmailAsync(googleUser.Email, ct);
        if (user == null)
        {
            user = User.CreateGoogle(googleUser.Email, googleUser.Name, googleUser.Sub, googleUser.Picture);
            await _users.AddAsync(user, ct);
            await _users.AssignRoleAsync(user.Id, RoleConstants.UserId, ct);
        }
        else
        {
            user.AvatarUrl = googleUser.Picture;
            if (user.ExternalProviderId == null) user.ExternalProviderId = googleUser.Sub;
        }
        await _users.SaveChangesAsync(ct);

        var role = await _users.GetRoleNameAsync(user.Id, ct) ?? RoleConstants.User;
        var permissions = await _users.GetPermissionsAsync(user.Id, ct);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, role, permissions);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var hashedToken = _tokenService.HashToken(rawRefreshToken);

        var refreshToken = RefreshToken.Create(user.Id, hashedToken, DateTime.UtcNow.AddDays(30));
        await _refreshTokens.AddAsync(refreshToken, ct);
        await _refreshTokens.SaveChangesAsync(ct);

        return (accessToken, rawRefreshToken, googleUser);
    }
}

public record GoogleUserInfo(string Sub, string Email, string Name, string? Picture);
