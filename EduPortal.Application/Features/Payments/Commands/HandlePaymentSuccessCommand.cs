using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using MediatR;

namespace EduPortal.Application.Features.Payments.Commands;

public record HandlePaymentSuccessCommand(
    Guid OrderId,
    string GatewayOrderId,
    string PaymentId,
    string? Signature,
    string EventId) : IRequest<Result>;

public class HandlePaymentSuccessCommandHandler : IRequestHandler<HandlePaymentSuccessCommand, Result>
{
    private readonly IOrderRepository _orders;
    private readonly IEnrollmentRepository _enrollments;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public HandlePaymentSuccessCommandHandler(
        IOrderRepository orders,
        IEnrollmentRepository enrollments,
        IEnumerable<IPaymentGateway> gateways)
    {
        _orders = orders;
        _enrollments = enrollments;
        _gateways = gateways;
    }

    public async Task<Result> Handle(HandlePaymentSuccessCommand request, CancellationToken cancellationToken)
    {
        // Idempotency: skip if event already processed
        var existing = await _orders.GetByGatewayEventIdAsync(request.EventId, cancellationToken);
        if (existing != null) return Result.Success();

        var order = await _orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null) return Result.NotFound("Order not found.");

        var gateway = _gateways.FirstOrDefault(g => g.GatewayName.Equals(order.GatewayName, StringComparison.OrdinalIgnoreCase));
        if (gateway == null) return Result.Failure("Gateway not found.", 400);

        var verified = await gateway.VerifyPaymentAsync(new VerifyPaymentRequest(
            order.GatewayName, request.GatewayOrderId, request.PaymentId, request.Signature), cancellationToken);

        if (!verified) return Result.Failure("Payment verification failed.", 400);

        order.Complete(request.PaymentId, request.EventId);

        var enrolled = await _enrollments.IsEnrolledAsync(order.UserId, order.ResourceId, cancellationToken);
        if (!enrolled)
        {
            await _enrollments.AddAsync(Enrollment.Create(order.UserId, order.ResourceId), cancellationToken);
        }

        await _orders.SaveChangesAsync(cancellationToken);
        await _enrollments.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
