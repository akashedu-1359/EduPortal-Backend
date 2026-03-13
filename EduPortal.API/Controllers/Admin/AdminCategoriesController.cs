using EduPortal.Application.Features.Categories.Commands;
using EduPortal.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Policy = "ContentWrite")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), ct);
        return Ok(new { success = true, data = result.Value });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? StatusCode(201, new { success = true, data = result.Value })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(id, req.Name, req.Description, req.IsVisible, req.SortOrder), ct);
        return result.IsSuccess ? Ok(new { success = true, data = result.Value })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id), ct);
        return result.IsSuccess ? Ok(new { success = true })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }
}

public record UpdateCategoryRequest(string Name, string? Description, bool IsVisible, int SortOrder);
