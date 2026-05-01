using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Domain.Common;

namespace MotorCare.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug(
                EventIdStore.Common.ExpectedRequestFailure,
                "Request canceled by client. Path={Path} CorrelationId={CorrelationId}",
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        catch (Exception exception)
        {
            await HandleAsync(httpContext, exception);
        }
    }

    private async Task HandleAsync(HttpContext httpContext, Exception exception)
    {
        var (statusCode, title, detail, errors) = MapException(exception, _environment.IsDevelopment());
        var logLevel = ExpectedExceptionClassifier.GetLogLevel(exception);
        var eventId = exception switch
        {
            AppValidationException => EventIdStore.Common.ValidationFailed,
            _ when ExpectedExceptionClassifier.IsExpected(exception) => EventIdStore.Common.ExpectedRequestFailure,
            _ => EventIdStore.Common.UnhandledException
        };

        _logger.Log(
            logLevel,
            eventId,
            exception,
            "Request failed. StatusCode={StatusCode} Title={Title} Path={Path} CorrelationId={CorrelationId}",
            statusCode,
            title,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

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
        await httpContext.Response.WriteAsJsonAsync(problemDetails, options);
    }

    private static (int StatusCode, string Title, string Detail, object? Errors) MapException(Exception exception, bool isDevelopment)
        => exception switch
        {
            DomainException domainEx =>
                (StatusCodes.Status422UnprocessableEntity, "Domain Rule Violation", domainEx.Message, null),
            ConflictException conflictEx =>
                (StatusCodes.Status409Conflict, "Conflict", conflictEx.Message, null),
            NotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound, "Not Found", notFoundEx.Message, null),
            AppValidationException appValidationEx =>
                (StatusCodes.Status422UnprocessableEntity, "Validation Failed", appValidationEx.Message, appValidationEx.Errors),
            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "Concurrency Conflict", "The record was modified by another user. Please reload and try again.", null),
            UnauthorizedAccessException unauthorizedEx =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", unauthorizedEx.Message, null),
            InvalidOperationException invalidOpEx =>
                (StatusCodes.Status400BadRequest, "Bad Request", invalidOpEx.Message, null),
            ArgumentException argEx =>
                (StatusCodes.Status400BadRequest, "Bad Request", argEx.Message, null),
            _ =>
                (StatusCodes.Status500InternalServerError, "Internal Server Error", isDevelopment ? exception.Message : "An unexpected error occurred. Please try again later.", null)
        };

    private static string GetRfcType(int statusCode)
        => statusCode switch
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
