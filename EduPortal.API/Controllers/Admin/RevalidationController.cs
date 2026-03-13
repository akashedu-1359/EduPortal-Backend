using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/cms")]
[Authorize(Policy = "CmsManage")]
public class RevalidationController : ControllerBase
{
    private readonly IRevalidationService _revalidation;

    public RevalidationController(IRevalidationService revalidation) => _revalidation = revalidation;

    [HttpPost("revalidate")]
    public async Task<IActionResult> Revalidate([FromQuery] string path, CancellationToken ct)
    {
        await _revalidation.TriggerRevalidationAsync(path, ct);
        return Ok(new { message = $"Revalidation triggered for: {path}" });
    }
}
