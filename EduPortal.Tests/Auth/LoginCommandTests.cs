using EduPortal.Application.Features.Auth.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Auth;

public class LoginCommandTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepo.Object, _refreshRepo.Object, _tokenService.Object, _passwordHasher.Object);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccess()
    {
        var user = User.CreateLocal("test@example.com", "hashedpwd", "Test User");
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("password123", "hashedpwd")).Returns(true);
        _userRepo.Setup(r => r.GetRoleNameAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync("User");
        _userRepo.Setup(r => r.GetPermissionsAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string>());
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns("access_token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("raw_refresh");
        _tokenService.Setup(t => t.HashToken("raw_refresh")).Returns("hashed_refresh");

        var result = await CreateHandler().Handle(new LoginCommand("test@example.com", "password123"), default);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("raw_refresh");
    }

    [Fact]
    public async Task Handle_WrongPassword_Returns401()
    {
        var user = User.CreateLocal("test@example.com", "hashedpwd", "Test User");
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("wrongpwd", "hashedpwd")).Returns(false);

        var result = await CreateHandler().Handle(new LoginCommand("test@example.com", "wrongpwd"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Handle_UserNotFound_Returns401()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new LoginCommand("notfound@example.com", "pwd"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Handle_InactiveUser_Returns403()
    {
        var user = User.CreateLocal("test@example.com", "hashedpwd", "Test User");
        user.IsActive = false;
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("password123", "hashedpwd")).Returns(true);

        var result = await CreateHandler().Handle(new LoginCommand("test@example.com", "password123"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
    }
}
