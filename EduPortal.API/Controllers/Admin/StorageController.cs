using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/storage")]
[Authorize(Policy = "ContentWrite")]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storage;

    public StorageController(IStorageService storage) => _storage = storage;

    [HttpPost("upload-url")]
    public async Task<IActionResult> GetUploadUrl([FromBody] GetUploadUrlRequest request, CancellationToken ct)
    {
        var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf", "video/mp4", "video/webm", "video/quicktime",
            "image/jpeg", "image/png", "image/webp"
        };

        if (!allowedTypes.Contains(request.ContentType))
            return BadRequest(new { success = false, error = "Unsupported content type." });

        var folder = request.ContentType.StartsWith("image/") ? "thumbnails"
            : request.ContentType.StartsWith("video/") ? "resources/video"
            : "resources/pdf";

        var fileKey = $"{folder}/{Guid.NewGuid()}/{request.FileName}";
        var uploadUrl = await _storage.GetUploadUrlAsync(fileKey, request.ContentType, 3600, ct);

        return Ok(new { success = true, data = new { uploadUrl, fileKey } });
    }
}

public record GetUploadUrlRequest(string FileName, string ContentType);
