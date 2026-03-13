using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EduPortal.Application.Features.Payments.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Webhooks;

[ApiController]
[Route("api/webhooks/razorpay")]
public class RazorpayWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly string _webhookSecret;

    public RazorpayWebhookController(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _webhookSecret = config["Payment:Razorpay:WebhookSecret"] ?? "";
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken ct)
    {
        string json;
        using (var reader = new StreamReader(HttpContext.Request.Body))
            json = await reader.ReadToEndAsync(ct);

        var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault() ?? "";
        if (!VerifySignature(json, signature))
            return BadRequest(new { error = "Invalid signature." });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("event", out var eventEl))
            return Ok();

        var eventName = eventEl.GetString();

        if (eventName == "payment.captured")
        {
            var entity = root
                .GetProperty("payload")
                .GetProperty("payment")
                .GetProperty("entity");

            var paymentId = entity.GetProperty("id").GetString()!;
            var gatewayOrderId = entity.GetProperty("order_id").GetString()!;

            if (entity.TryGetProperty("notes", out var notesEl) &&
                notesEl.TryGetProperty("orderId", out var orderIdNote) &&
                Guid.TryParse(orderIdNote.GetString(), out var internalOrderId))
            {
                await _mediator.Send(
                    new HandlePaymentSuccessCommand(internalOrderId, gatewayOrderId, paymentId, null, paymentId),
                    ct);
            }
        }

        return Ok();
    }

    private bool VerifySignature(string payload, string signature)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expected = Convert.ToHexString(hash).ToLower();
        return expected == signature;
    }
}
