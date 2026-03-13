using System.Security.Cryptography;
using System.Text;
using EduPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;

namespace EduPortal.Infrastructure.Services;

public class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly string _keyId;
    private readonly string _keySecret;

    public string GatewayName => "Razorpay";

    public RazorpayPaymentGateway(IConfiguration configuration)
    {
        _keyId = configuration["Payment:Razorpay:KeyId"] ?? "";
        _keySecret = configuration["Payment:Razorpay:KeySecret"] ?? "";
    }

    public Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var client = new RazorpayClient(_keyId, _keySecret);
        var options = new Dictionary<string, object>
        {
            { "amount", (long)(request.Amount * 100) },
            { "currency", request.Currency },
            { "receipt", request.ReceiptId }
        };
        var order = client.Order.Create(options);
        var orderId = order["id"].ToString()!;
        return Task.FromResult(new CreateOrderResponse(orderId, null));
    }

    public Task<bool> VerifyPaymentAsync(VerifyPaymentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(request.Signature)) return Task.FromResult(false);
        var payload = $"{request.GatewayOrderId}|{request.PaymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computed = Convert.ToHexString(hash).ToLower();
        return Task.FromResult(computed == request.Signature);
    }
}
