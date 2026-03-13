using EduPortal.Application.Features.Certificates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.User;

[ApiController]
[Route("api/user/certificates")]
[Authorize]
public class UserCertificatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserCertificatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetMyCertificates(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyCertificatesQuery(), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCertificateDownloadQuery(id), ct);
        return result.IsSuccess ? Ok(new { url = result.Value }) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
