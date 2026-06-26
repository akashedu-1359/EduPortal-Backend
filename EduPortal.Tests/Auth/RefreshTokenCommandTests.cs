using EduPortal.Application.Features.Auth.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Auth;

public class RefreshTokenCommandTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<ICacheService> _cache = new();

    private RefreshTokenCommandHandler CreateHandler() =>
        new(_userRepo.Object, _refreshRepo.Object, _tokenService.Object, _cache.Object);

    [Fact]
    public async Task Handle_ValidToken_RotatesAndReturnsNewTokens()
    {
        var user = User.CreateLocal("test@example.com", "hash", "Test User");
        var storedToken = RefreshToken.Create(user.Id, "hashed_old", DateTime.UtcNow.AddDays(30));

        _tokenService.Setup(t => t.HashToken("raw_refresh")).Returns("hashed_old");
        _cache.Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((bool?)null);
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_old", It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.GetRoleNameAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync("User");
        _userRepo.Setup(r => r.GetPermissionsAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string>());
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("new_raw_refresh");
        _tokenService.Setup(t => t.HashToken("new_raw_refresh")).Returns("new_hashed_refresh");
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns("new_access_token");

        var result = await CreateHandler().Handle(new RefreshTokenCommand("raw_refresh"), default);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.AccessToken.Should().Be("new_access_token");
        result.Value.RefreshToken.Should().Be("new_raw_refresh");
        _refreshRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RevokedTokenInCache_Returns401()
    {
        _tokenService.Setup(t => t.HashToken("revoked_token")).Returns("hashed_revoked");
        _cache.Setup(c => c.GetAsync<bool?>("revoked_token:hashed_revoked", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("revoked_token"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Handle_TokenNotFoundInDb_Returns401()
    {
        _tokenService.Setup(t => t.HashToken("unknown")).Returns("hashed_unknown");
        _cache.Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((bool?)null);
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_unknown", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("unknown"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Handle_ExpiredToken_Returns401()
    {
        var storedToken = RefreshToken.Create(Guid.NewGuid(), "hashed_expired", DateTime.UtcNow.AddDays(-1));

        _tokenService.Setup(t => t.HashToken("expired")).Returns("hashed_expired");
        _cache.Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((bool?)null);
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_expired", It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("expired"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Handle_InactiveUser_Returns401()
    {
        var user = User.CreateLocal("test@example.com", "hash", "Test User");
        user.IsActive = false;
        var storedToken = RefreshToken.Create(user.Id, "hashed_token", DateTime.UtcNow.AddDays(30));

        _tokenService.Setup(t => t.HashToken("raw")).Returns("hashed_token");
        _cache.Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((bool?)null);
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_token", It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("raw"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Handle_UserNotFound_Returns401()
    {
        var storedToken = RefreshToken.Create(Guid.NewGuid(), "hashed_token", DateTime.UtcNow.AddDays(30));

        _tokenService.Setup(t => t.HashToken("raw")).Returns("hashed_token");
        _cache.Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((bool?)null);
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_token", It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);
        _userRepo.Setup(r => r.GetByIdAsync(storedToken.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("raw"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }
}
