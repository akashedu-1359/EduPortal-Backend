using EduPortal.Application.Features.Resources.Commands;
using EduPortal.Application.Features.Resources.Queries;
using EduPortal.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/resources")]
[Authorize(Policy = "ContentWrite")]
public class AdminResourcesController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminResourcesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        [FromQuery] ResourceStatus? status = null, [FromQuery] Guid? categoryId = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminResourcesQuery(pageNumber, pageSize, status, categoryId), ct);
        return Ok(new { success = true, data = result.Value });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceRequest req, CancellationToken ct)
    {
        var price = req.PricingType == "Free" ? 0m : (req.Price ?? 0m);
        var command = new CreateResourceCommand(req.Title, req.Description, req.Type, null, null, null, null, price, req.CategoryId);
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? StatusCode(201, new { success = true, data = result.Value })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateResourceCommand(id, req.Title, req.Description, req.FileKey, req.ExternalUrl, req.BlogContent, req.ThumbnailKey, req.Price, req.IsFeatured, req.CategoryId), ct);
        return result.IsSuccess ? Ok(new { success = true, data = result.Value })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteResourceCommand(id), ct);
        return result.IsSuccess ? Ok(new { success = true })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishResourceCommand(id), ct);
        return result.IsSuccess ? Ok(new { success = true })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ArchiveResourceCommand(id), ct);
        return result.IsSuccess ? Ok(new { success = true })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }
}

public record CreateResourceRequest(
    string Title, string Description, ResourceType Type, Guid CategoryId,
    string PricingType, decimal? Price, string? Currency, string[]? Tags, int? DurationMinutes);

public record UpdateResourceRequest(string Title, string Description, string? FileKey, string? ExternalUrl,
    string? BlogContent, string? ThumbnailKey, decimal Price, bool IsFeatured, Guid CategoryId);
