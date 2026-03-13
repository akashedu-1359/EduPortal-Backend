using EduPortal.Application.Features.Payments.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace EduPortal.API.Controllers.Webhooks;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly string _webhookSecret;

    public StripeWebhookController(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _webhookSecret = config["Payment:Stripe:WebhookSecret"] ?? "";
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken ct)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _webhookSecret);

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                var intent = (PaymentIntent)stripeEvent.Data.Object;
                var orderId = intent.Metadata.TryGetValue("orderId", out var oid) && Guid.TryParse(oid, out var parsed)
                    ? parsed
                    : Guid.Empty;

                await _mediator.Send(
                    new HandlePaymentSuccessCommand(orderId, intent.Id, intent.Id, null, stripeEvent.Id),
                    ct);
            }

            return Ok();
        }
        catch (StripeException)
        {
            return BadRequest();
        }
    }
}
