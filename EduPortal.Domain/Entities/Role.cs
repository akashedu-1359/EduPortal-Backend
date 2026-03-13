namespace EduPortal.Domain.Entities;

public class Role
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Role() { }

    public Role(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}
