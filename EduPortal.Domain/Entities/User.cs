using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;

namespace EduPortal.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = default!;
    public string? PasswordHash { get; private set; }
    public string FullName { get; private set; } = default!;
    public string? AvatarUrl { get; set; }
    public AuthProvider AuthProvider { get; private set; }
    public string? ExternalProviderId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();
    public ICollection<Order> Orders { get; private set; } = new List<Order>();
    public ICollection<ExamAttempt> ExamAttempts { get; private set; } = new List<ExamAttempt>();

    private User() { }

    public static User CreateLocal(string email, string passwordHash, string fullName)
    {
        return new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName,
            AuthProvider = AuthProvider.Local
        };
    }

    public static User CreateGoogle(string email, string fullName, string externalProviderId, string? avatarUrl)
    {
        return new User
        {
            Email = email.ToLowerInvariant(),
            FullName = fullName,
            ExternalProviderId = externalProviderId,
            AvatarUrl = avatarUrl,
            AuthProvider = AuthProvider.Google
        };
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
