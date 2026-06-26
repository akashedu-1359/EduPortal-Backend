using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentAssertions;

namespace EduPortal.Tests.Domain;

public class UserTests
{
    [Fact]
    public void CreateLocal_SetsCorrectProperties()
    {
        var user = User.CreateLocal("Test@Example.COM", "hashedpwd", "Test User");

        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hashedpwd");
        user.FullName.Should().Be("Test User");
        user.AuthProvider.Should().Be(AuthProvider.Local);
        user.IsActive.Should().BeTrue();
        user.ExternalProviderId.Should().BeNull();
        user.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public void CreateLocal_NormalizesEmailToLowerCase()
    {
        var user = User.CreateLocal("USER@DOMAIN.COM", "hash", "Name");

        user.Email.Should().Be("user@domain.com");
    }

    [Fact]
    public void CreateGoogle_SetsCorrectProperties()
    {
        var user = User.CreateGoogle("oauth@gmail.com", "OAuth User", "google-id-123", "https://avatar.url/photo.jpg");

        user.Email.Should().Be("oauth@gmail.com");
        user.FullName.Should().Be("OAuth User");
        user.AuthProvider.Should().Be(AuthProvider.Google);
        user.ExternalProviderId.Should().Be("google-id-123");
        user.AvatarUrl.Should().Be("https://avatar.url/photo.jpg");
        user.PasswordHash.Should().BeNull();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateGoogle_WithNullAvatar_SetsAvatarToNull()
    {
        var user = User.CreateGoogle("oauth@gmail.com", "OAuth User", "google-id-123", null);

        user.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public void IsActive_CanBeToggledOff()
    {
        var user = User.CreateLocal("test@example.com", "hash", "Name");
        user.IsActive.Should().BeTrue();

        user.IsActive = false;

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_CanBeToggledBackOn()
    {
        var user = User.CreateLocal("test@example.com", "hash", "Name");
        user.IsActive = false;

        user.IsActive = true;

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdatePassword_ChangesHashAndUpdatesTimestamp()
    {
        var user = User.CreateLocal("test@example.com", "old_hash", "Name");
        var originalUpdatedAt = user.UpdatedAt;

        user.UpdatePassword("new_hash");

        user.PasswordHash.Should().Be("new_hash");
        user.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void CreateLocal_GeneratesUniqueId()
    {
        var user1 = User.CreateLocal("a@b.com", "hash", "User 1");
        var user2 = User.CreateLocal("c@d.com", "hash", "User 2");

        user1.Id.Should().NotBe(user2.Id);
        user1.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateLocal_InitializesEmptyCollections()
    {
        var user = User.CreateLocal("test@example.com", "hash", "Name");

        user.UserRoles.Should().BeEmpty();
        user.RefreshTokens.Should().BeEmpty();
        user.Enrollments.Should().BeEmpty();
        user.Orders.Should().BeEmpty();
        user.ExamAttempts.Should().BeEmpty();
    }
}
