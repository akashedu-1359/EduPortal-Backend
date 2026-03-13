using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentAssertions;

namespace EduPortal.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_SetsCorrectInitialValues()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        var order = Order.Create(userId, resourceId, 999m, "INR", "stripe");

        order.UserId.Should().Be(userId);
        order.ResourceId.Should().Be(resourceId);
        order.Amount.Should().Be(999m);
        order.Currency.Should().Be("INR");
        order.GatewayName.Should().Be("stripe");
        order.Status.Should().Be(OrderStatus.Pending);
        order.GatewayTransactionId.Should().BeNull();
        order.GatewayEventId.Should().BeNull();
    }

    [Fact]
    public void Complete_UpdatesStatusAndIds()
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), 999m, "INR", "stripe");

        order.Complete("txn_123", "evt_456");

        order.Status.Should().Be(OrderStatus.Completed);
        order.GatewayTransactionId.Should().Be("txn_123");
        order.GatewayEventId.Should().Be("evt_456");
    }
}
