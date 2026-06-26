using EduPortal.Application.Features.Auth.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Auth;

public class LogoutCommandTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<ICacheService> _cache = new();

    private LogoutCommandHandler CreateHandler() =>
        new(_refreshRepo.Object, _tokenService.Object, _cache.Object);

    [Fact]
    public async Task Handle_ValidToken_RevokesAndReturnsSuccess()
    {
        var storedToken = RefreshToken.Create(Guid.NewGuid(), "hashed_token", DateTime.UtcNow.AddDays(30));
        _tokenService.Setup(t => t.HashToken("raw_token")).Returns("hashed_token");
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_token", It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);

        var result = await CreateHandler().Handle(new LogoutCommand("raw_token"), default);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        _refreshRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TokenNotFound_StillReturnsSuccess()
    {
        _tokenService.Setup(t => t.HashToken("unknown")).Returns("hashed_unknown");
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_unknown", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        var result = await CreateHandler().Handle(new LogoutCommand("unknown"), default);

        result.IsSuccess.Should().BeTrue();
        _refreshRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyRevokedToken_ReturnsSuccessWithoutDoubleRevoke()
    {
        var storedToken = RefreshToken.Create(Guid.NewGuid(), "hashed_token", DateTime.UtcNow.AddDays(30));
        storedToken.Revoke();
        _tokenService.Setup(t => t.HashToken("raw")).Returns("hashed_token");
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_token", It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);

        var result = await CreateHandler().Handle(new LogoutCommand("raw"), default);

        result.IsSuccess.Should().BeTrue();
        _refreshRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidToken_AddsToCacheRevocationList()
    {
        var storedToken = RefreshToken.Create(Guid.NewGuid(), "hashed_token", DateTime.UtcNow.AddDays(30));
        _tokenService.Setup(t => t.HashToken("raw_token")).Returns("hashed_token");
        _refreshRepo.Setup(r => r.GetByTokenAsync("hashed_token", It.IsAny<CancellationToken>())).ReturnsAsync(storedToken);

        await CreateHandler().Handle(new LogoutCommand("raw_token"), default);

        _cache.Verify(c => c.SetAsync("revoked_token:hashed_token", true, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
