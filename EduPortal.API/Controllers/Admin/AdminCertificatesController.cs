using EduPortal.Application.Features.Certificates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/certificates")]
[Authorize(Policy = "ExamManage")]
public class AdminCertificatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCertificatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminCertificatesQuery(page, pageSize, search), ct);
        return Ok(new { success = true, data = result.Value });
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminCertificateDownloadQuery(id), ct);
        return result.IsSuccess
            ? Ok(new { success = true, data = new { url = result.Value } })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }
}
