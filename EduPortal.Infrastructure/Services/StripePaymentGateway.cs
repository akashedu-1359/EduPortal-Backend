using EduPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace EduPortal.Infrastructure.Services;

public class StripePaymentGateway : IPaymentGateway
{
    public string GatewayName => "Stripe";

    public StripePaymentGateway(IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = configuration["Payment:Stripe:SecretKey"] ?? "";
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100),
            Currency = request.Currency.ToLower(),
            Metadata = request.Metadata?.ToDictionary(k => k.Key, v => v.Value),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options, cancellationToken: ct);
        return new CreateOrderResponse(intent.Id, intent.ClientSecret);
    }

    public async Task<bool> VerifyPaymentAsync(VerifyPaymentRequest request, CancellationToken ct = default)
    {
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(request.GatewayOrderId, cancellationToken: ct);
        return intent.Status == "succeeded";
    }
}
