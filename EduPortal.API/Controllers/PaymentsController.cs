using EduPortal.Application.Features.Payments.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? StatusCode(result.StatusCode, result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] HandlePaymentSuccessCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { message = "Payment verified and enrollment confirmed." })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
