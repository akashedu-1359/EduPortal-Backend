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
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] ResourceStatus? status = null, [FromQuery] Guid? categoryId = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminResourcesQuery(page, pageSize, status, categoryId), ct);
        return Ok(new { success = true, data = result.Value });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceCommand command, CancellationToken ct)
    {
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

    [HttpPatch("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishResourceCommand(id), ct);
        return result.IsSuccess ? Ok(new { success = true })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpPatch("{id:guid}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new UnpublishResourceCommand(id), ct);
        return result.IsSuccess ? Ok(new { success = true })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }
}

public record UpdateResourceRequest(string Title, string Description, string? FileKey, string? ExternalUrl,
    string? BlogContent, string? ThumbnailKey, decimal Price, bool IsFeatured, Guid CategoryId);
