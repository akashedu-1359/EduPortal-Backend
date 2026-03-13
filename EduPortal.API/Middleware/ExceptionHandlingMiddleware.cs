using System.Text.Json;
using EduPortal.Domain.Exceptions;

namespace EduPortal.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code) = ex switch
        {
            NotFoundException => (404, "NOT_FOUND"),
            UnauthorizedException => (401, "UNAUTHORIZED"),
            BusinessRuleException => (422, "BUSINESS_RULE_VIOLATION"),
            ConflictException => (409, "CONFLICT"),
            _ => (500, "INTERNAL_SERVER_ERROR")
        };

        if (statusCode == 500)
            _logger.LogError(ex, "Unhandled exception at {Path}", context.Request.Path);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            error = new { code, message = ex.Message }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
