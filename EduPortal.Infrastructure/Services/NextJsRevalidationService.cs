using EduPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class NextJsRevalidationService : IRevalidationService
{
    private readonly HttpClient _http;
    private readonly string _frontendUrl;
    private readonly string _secret;
    private readonly ILogger<NextJsRevalidationService> _logger;

    public NextJsRevalidationService(HttpClient http, IConfiguration config, ILogger<NextJsRevalidationService> logger)
    {
        _http = http;
        _frontendUrl = config["Frontend:BaseUrl"] ?? "http://localhost:3000";
        _secret = config["Frontend:RevalidationSecret"] ?? "";
        _logger = logger;
    }

    public async Task TriggerRevalidationAsync(string tag, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_frontendUrl}/api/revalidate");
            request.Headers.Add("x-revalidation-secret", _secret);
            request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new { tag }),
                System.Text.Encoding.UTF8, "application/json");
            await _http.SendAsync(request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ISR revalidation failed for tag {Tag}", tag);
        }
    }
}
