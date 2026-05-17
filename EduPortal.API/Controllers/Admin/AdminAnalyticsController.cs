using EduPortal.Application.Features.Analytics.Queries;
using EduPortal.Application.Interfaces;
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
    private readonly IAnalyticsRepository _analytics;

    public AdminAnalyticsController(IMediator mediator, IAnalyticsRepository analytics)
    {
        _mediator = mediator;
        _analytics = analytics;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminDashboardQuery(), ct);
        return Ok(new { success = true, data = result.Value });
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var rows = await _analytics.GetRevenueByDayAsync(days, ct);
        var data = rows.Select(r => new { date = r.Date.ToString("yyyy-MM-dd"), amount = r.Amount });
        return Ok(new { success = true, data });
    }

    [HttpGet("enrollments")]
    public async Task<IActionResult> GetEnrollments([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var rows = await _analytics.GetEnrollmentsByDayAsync(days, ct);
        var data = rows.Select(r => new { date = r.Date.ToString("yyyy-MM-dd"), count = r.Count });
        return Ok(new { success = true, data });
    }
}
