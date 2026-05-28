using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Errors;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Api.Logging;
using MotorCare.Domain.Common;

namespace MotorCare.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
                RequestPathRedactor.Redact(httpContext.Request.Path),
                httpContext.TraceIdentifier);
        }
        catch (Exception exception)
        {
            await HandleAsync(httpContext, exception);
        }
    }

    private async Task HandleAsync(HttpContext httpContext, Exception exception)
    {
        var (statusCode, code, message, errors) = MapException(exception);
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
            "Request failed. StatusCode={StatusCode} Code={Code} Path={Path} CorrelationId={CorrelationId}",
            statusCode,
            code,
            RequestPathRedactor.Redact(httpContext.Request.Path),
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var responseBody = new
        {
            code,
            message,
            errors,
            traceId = httpContext.TraceIdentifier
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await httpContext.Response.WriteAsJsonAsync(responseBody, options);
    }

    private static (int StatusCode, string Code, string Message, object? Errors) MapException(Exception exception)
        => exception switch
        {
            LoginException loginEx =>
                (StatusCodes.Status401Unauthorized, loginEx.Code, loginEx.UserMessage, null),

            AppValidationException appValidationEx =>
                (StatusCodes.Status422UnprocessableEntity, ErrorCodes.ValidationError,
                    "Lütfen form alanlarını kontrol edin.", appValidationEx.Errors),

            DomainException domainEx =>
                (StatusCodes.Status422UnprocessableEntity, ErrorCodes.DomainRuleViolation, domainEx.Message, null),

            ConflictException conflictEx =>
                (StatusCodes.Status409Conflict, ErrorCodes.Conflict, conflictEx.Message, null),

            NotFoundException =>
                (StatusCodes.Status404NotFound, ErrorCodes.NotFound, "İstenen kayıt bulunamadı.", null),

            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, ErrorCodes.Conflict,
                    "Kayıt başka bir kullanıcı tarafından değiştirildi. Lütfen sayfayı yenileyip tekrar deneyin.", null),

            DbUpdateException =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.UnexpectedError,
                    "Şu anda sistemsel bir sorun oluştu. Lütfen daha sonra tekrar deneyin.", null),

            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized,
                    "Oturumunuzun süresi dolmuş olabilir. Lütfen tekrar giriş yapın.", null),

            _ when IsDbDriverException(exception) =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.UnexpectedError,
                    "Şu anda sistemsel bir sorun oluştu. Lütfen daha sonra tekrar deneyin.", null),

            _ =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.UnexpectedError,
                    "Şu anda işlem sırasında bir sorun oluştu. Lütfen daha sonra tekrar deneyin.", null)
        };

    private static bool IsDbDriverException(Exception ex) =>
        ex.GetType().Namespace?.StartsWith("Npgsql") == true ||
        ex.InnerException?.GetType().Namespace?.StartsWith("Npgsql") == true;
}
