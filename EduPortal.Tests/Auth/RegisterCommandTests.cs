using EduPortal.Application.Features.Auth.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Auth;

public class RegisterCommandTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    private RegisterCommandHandler CreateHandler() =>
        new(_userRepo.Object, _tokenService.Object, _passwordHasher.Object);

    [Fact]
    public async Task Handle_NewEmail_CreatesUserAndReturns201()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync("new@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash("password123")).Returns("hashed");
        _userRepo.Setup(r => r.GetPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<string>());
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns("access_token");

        var result = await CreateHandler().Handle(new RegisterCommand("new@example.com", "password123", "New User"), default);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Value!.AccessToken.Should().Be("access_token");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_Returns409()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync("existing@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateHandler().Handle(new RegisterCommand("existing@example.com", "password123", "User"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
