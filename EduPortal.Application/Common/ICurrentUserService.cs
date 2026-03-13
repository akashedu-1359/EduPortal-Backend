namespace EduPortal.Application.Common;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    IEnumerable<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
