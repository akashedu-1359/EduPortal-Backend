using EduPortal.Application.Features.Analytics.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Policy = "AnalyticsView")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAnalyticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminDashboardQuery(), ct);
        return Ok(result.Value);
    }
}
