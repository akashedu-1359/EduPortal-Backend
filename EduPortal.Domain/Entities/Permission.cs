namespace EduPortal.Domain.Entities;

public class Permission
{
    public int Id { get; private set; }
    public string Key { get; private set; } = default!;
    public string Description { get; private set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Permission() { }

    public Permission(int id, string key, string description)
    {
        Id = id;
        Key = key;
        Description = description;
    }
}
