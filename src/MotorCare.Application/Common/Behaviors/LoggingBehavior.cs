using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Exceptions;

namespace MotorCare.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            EventIdStore.Common.RequestStarted,
            "Handling {RequestName}",
            requestName);

        try
        {
            var response = await next();
            sw.Stop();

            _logger.LogInformation(
                EventIdStore.Common.RequestCompleted,
                "Completed {RequestName} in {ElapsedMs}ms",
                requestName,
                sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug(
                    EventIdStore.Common.ExpectedRequestFailure,
                    "Request {RequestName} canceled after {ElapsedMs}ms",
                    requestName,
                    sw.ElapsedMilliseconds);

                throw;
            }

            var logLevel = ExpectedExceptionClassifier.GetLogLevel(ex);
            var eventId = ex switch
            {
                AppValidationException => EventIdStore.Common.ValidationFailed,
                _ when ExpectedExceptionClassifier.IsExpected(ex) => EventIdStore.Common.ExpectedRequestFailure,
                _ => EventIdStore.Common.UnhandledException
            };

            _logger.Log(
                logLevel,
                eventId,
                ex,
                "Request {RequestName} failed after {ElapsedMs}ms",
                requestName,
                sw.ElapsedMilliseconds);

            throw;
        }
    }
}
