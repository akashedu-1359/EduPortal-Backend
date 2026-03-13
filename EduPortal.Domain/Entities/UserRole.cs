namespace EduPortal.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; private set; }
    public int RoleId { get; private set; }

    public User User { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    private UserRole() { }

    public UserRole(Guid userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
