namespace EduPortal.Application.Interfaces;

public record CreateOrderRequest(string GatewayName, decimal Amount, string Currency, string ReceiptId, IDictionary<string, string>? Metadata = null);
public record CreateOrderResponse(string GatewayOrderId, string? ClientSecret);
public record VerifyPaymentRequest(string GatewayName, string GatewayOrderId, string PaymentId, string? Signature);

public interface IPaymentGateway
{
    string GatewayName { get; }
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task<bool> VerifyPaymentAsync(VerifyPaymentRequest request, CancellationToken ct = default);
}
