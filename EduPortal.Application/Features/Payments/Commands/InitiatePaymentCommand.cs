using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Payments.Commands;

public record InitiatePaymentCommand(Guid ResourceId, string GatewayName) : IRequest<Result<InitiatePaymentResponse>>;
public record InitiatePaymentResponse(Guid OrderId, string GatewayOrderId, string? ClientSecret);

public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, Result<InitiatePaymentResponse>>
{
    private readonly IResourceRepository _resources;
    private readonly IOrderRepository _orders;
    private readonly IEnrollmentRepository _enrollments;
    private readonly ICurrentUserService _currentUser;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public InitiatePaymentCommandHandler(
        IResourceRepository resources,
        IOrderRepository orders,
        IEnrollmentRepository enrollments,
        ICurrentUserService currentUser,
        IEnumerable<IPaymentGateway> gateways)
    {
        _resources = resources;
        _orders = orders;
        _enrollments = enrollments;
        _currentUser = currentUser;
        _gateways = gateways;
    }

    public async Task<Result<InitiatePaymentResponse>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;

        var resource = await _resources.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource == null) return Result<InitiatePaymentResponse>.NotFound("Resource not found.");
        if (resource.Status != ResourceStatus.Published) return Result<InitiatePaymentResponse>.Failure("Resource is not available.");
        if (resource.Price == 0) return Result<InitiatePaymentResponse>.Failure("This resource is free. Use the enroll endpoint.");

        var alreadyEnrolled = await _enrollments.IsEnrolledAsync(userId, request.ResourceId, cancellationToken);
        if (alreadyEnrolled) return Result<InitiatePaymentResponse>.Conflict("Already enrolled in this resource.");

        var gateway = _gateways.FirstOrDefault(g => g.GatewayName.Equals(request.GatewayName, StringComparison.OrdinalIgnoreCase));
        if (gateway == null) return Result<InitiatePaymentResponse>.Failure($"Payment gateway '{request.GatewayName}' not supported.", 400);

        var order = Order.Create(userId, request.ResourceId, resource.Price, "INR", gateway.GatewayName);
        await _orders.AddAsync(order, cancellationToken);
        await _orders.SaveChangesAsync(cancellationToken);

        var gatewayReq = new CreateOrderRequest(
            gateway.GatewayName,
            resource.Price,
            "INR",
            order.Id.ToString(),
            new Dictionary<string, string> { { "orderId", order.Id.ToString() } });

        var gatewayResp = await gateway.CreateOrderAsync(gatewayReq, cancellationToken);

        return Result<InitiatePaymentResponse>.Created(new InitiatePaymentResponse(order.Id, gatewayResp.GatewayOrderId, gatewayResp.ClientSecret));
    }
}
