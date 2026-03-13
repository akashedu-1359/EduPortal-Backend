namespace EduPortal.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AdminUserId { get; private set; }
    public string EntityType { get; private set; } = default!;
    public string EntityId { get; private set; } = default!;
    public string Action { get; private set; } = default!;
    public string? OldValueJson { get; private set; }
    public string? NewValueJson { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public static AuditLog Create(Guid adminId, string entityType, string entityId, string action, string? oldJson = null, string? newJson = null)
    {
        return new AuditLog
        {
            AdminUserId = adminId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValueJson = oldJson,
            NewValueJson = newJson
        };
    }
}
