using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities;

public class Enrollment : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid? OrderId { get; private set; }
    public DateTime EnrolledAt { get; private set; }

    public User User { get; private set; } = default!;
    public Resource Resource { get; private set; } = default!;
    public Order? Order { get; private set; }

    private Enrollment() { }

    public static Enrollment Create(Guid userId, Guid resourceId, Guid? orderId = null)
    {
        return new Enrollment
        {
            UserId = userId,
            ResourceId = resourceId,
            OrderId = orderId,
            EnrolledAt = DateTime.UtcNow
        };
    }
}
