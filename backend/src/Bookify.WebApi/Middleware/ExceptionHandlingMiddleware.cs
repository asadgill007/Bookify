using System.Net;
using System.Text.Json;
using Bookify.Domain.Common;
using FluentValidation;

namespace Bookify.WebApi.Middleware;

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
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (DomainException ex)
        {
            await HandleDomainExceptionAsync(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedAccessExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleInternalServerErrorAsync(context, ex);
        }
    }

    private static string GetCorrelationId(HttpContext context)
        => context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
           ?? context.TraceIdentifier
           ?? "none";

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        var correlationId = GetCorrelationId(context);
        _logger.LogWarning("Validation failed for {CorrelationId}: {Errors}", correlationId, exception.Errors);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        var response = new
        {
            type = "https://httpstatuses.com/422",
            title = "Validation Failed",
            status = 422,
            detail = "One or more validation errors occurred.",
            instance = context.Request.Path,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private async Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
    {
        var correlationId = GetCorrelationId(context);
        _logger.LogWarning("Domain rule violated for {CorrelationId}: {Message}", correlationId, exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            type = "https://httpstatuses.com/400",
            title = "Business Rule Violation",
            status = 400,
            detail = exception.Message,
            instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleNotFoundExceptionAsync(HttpContext context, KeyNotFoundException exception)
    {
        var correlationId = GetCorrelationId(context);
        _logger.LogWarning("Resource not found for {CorrelationId}: {Message}", correlationId, exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var response = new
        {
            type = "https://httpstatuses.com/404",
            title = "Resource Not Found",
            status = 404,
            detail = exception.Message,
            instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleUnauthorizedAccessExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
    {
        var correlationId = GetCorrelationId(context);
        _logger.LogWarning("Unauthorized access for {CorrelationId}: {Message}", correlationId, exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        var response = new
        {
            type = "https://httpstatuses.com/403",
            title = "Forbidden",
            status = 403,
            detail = "You do not have permission to perform this action.",
            instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleInternalServerErrorAsync(HttpContext context, Exception exception)
    {
        var correlationId = GetCorrelationId(context);
        _logger.LogError(exception, "Unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            type = "https://httpstatuses.com/500",
            title = "An error occurred while processing your request.",
            status = 500,
            detail = "Internal server error. Please try again later.",
            instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
