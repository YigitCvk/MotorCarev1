using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Domain.Common;

namespace MotorCare.Api.Middleware;

/// <summary>
/// Global exception handler implementing IExceptionHandler (.NET 8).
/// Maps domain and application exceptions to appropriate HTTP status codes.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, errors) = MapException(exception);

        _logger.LogError(exception, "Unhandled exception. StatusCode={StatusCode}, Title={Title}", statusCode, title);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type = GetRfcType(statusCode),
            title,
            status = statusCode,
            detail,
            errors,
            traceId = httpContext.TraceIdentifier
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await httpContext.Response.WriteAsJsonAsync(problemDetails, options, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail, object? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            // Domain business rule violation → 422
            DomainException domainEx =>
                (StatusCodes.Status422UnprocessableEntity,
                 "Domain Rule Violation",
                 domainEx.Message,
                 null),

            // Application-level conflict (duplicate data) → 409
            ConflictException conflictEx =>
                (StatusCodes.Status409Conflict,
                 "Conflict",
                 conflictEx.Message,
                 null),

            // Entity not found → 404
            NotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound,
                 "Not Found",
                 notFoundEx.Message,
                 null),

            // Application validation exception → 422
            AppValidationException appValidationEx =>
                (StatusCodes.Status422UnprocessableEntity,
                 "Validation Failed",
                 appValidationEx.Message,
                 appValidationEx.Errors),

            // EF Core optimistic concurrency → 409
            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict,
                 "Concurrency Conflict",
                 "The record was modified by another user. Please reload and try again.",
                 null),

            // Unauthorized → 401
            UnauthorizedAccessException unauthorizedEx =>
                (StatusCodes.Status401Unauthorized,
                 "Unauthorized",
                 unauthorizedEx.Message,
                 null),

            // Invalid operation (programming errors) → 400
            InvalidOperationException invalidOpEx =>
                (StatusCodes.Status400BadRequest,
                 "Bad Request",
                 invalidOpEx.Message,
                 null),

            // ArgumentException → 400
            ArgumentException argEx =>
                (StatusCodes.Status400BadRequest,
                 "Bad Request",
                 argEx.Message,
                 null),

            // Catch-all → 500
            _ =>
                (StatusCodes.Status500InternalServerError,
                 "Internal Server Error",
                 "An unexpected error occurred. Please try again later.",
                 null)
        };
    }

    private static string GetRfcType(int statusCode)
    {
        return statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            422 => "https://tools.ietf.org/html/rfc9110#section-15.5.21",
            500 => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            _ => "https://tools.ietf.org/html/rfc9110"
        };
    }
}
