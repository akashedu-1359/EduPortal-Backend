using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/storage")]
[Authorize(Policy = "ContentWrite")]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storage;
    private readonly string _publicUrl;

    public StorageController(IStorageService storage, IConfiguration config)
    {
        _storage = storage;
        _publicUrl = config["Storage:PublicUrl"] ?? string.Empty;
    }

    [HttpPost("presigned-upload")]
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

        var key = $"{folder}/{Guid.NewGuid()}/{request.FileName}";
        var uploadUrl = await _storage.GetUploadUrlAsync(key, request.ContentType, 3600, ct);
        var publicUrl = string.IsNullOrEmpty(_publicUrl) ? string.Empty : $"{_publicUrl.TrimEnd('/')}/{key}";

        return Ok(new { success = true, data = new { uploadUrl, publicUrl, key } });
    }
}

public record GetUploadUrlRequest(string FileName, string ContentType, string? Purpose);
