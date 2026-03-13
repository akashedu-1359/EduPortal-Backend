using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;

namespace EduPortal.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ResourceId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "INR";
    public string GatewayName { get; private set; } = default!;
    public string? GatewayTransactionId { get; set; }
    public string? GatewayEventId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public User User { get; private set; } = default!;
    public Resource Resource { get; private set; } = default!;

    private Order() { }

    public static Order Create(Guid userId, Guid resourceId, decimal amount, string currency, string gatewayName)
    {
        return new Order
        {
            UserId = userId,
            ResourceId = resourceId,
            Amount = amount,
            Currency = currency,
            GatewayName = gatewayName
        };
    }

    public void Complete(string transactionId, string eventId)
    {
        GatewayTransactionId = transactionId;
        GatewayEventId = eventId;
        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }
}
