using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Domain.Common;

namespace MotorCare.Application.Common;

public static class ExpectedExceptionClassifier
{
    public static bool IsExpected(Exception exception)
        => exception is AppValidationException
            or UnauthorizedAccessException
            or NotFoundException
            or ConflictException
            or DomainException
            or InvalidOperationException
            or ArgumentException;

    public static LogLevel GetLogLevel(Exception exception)
        => exception switch
        {
            AppValidationException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            NotFoundException => LogLevel.Information,
            ConflictException => LogLevel.Warning,
            DomainException => LogLevel.Warning,
            InvalidOperationException => LogLevel.Warning,
            ArgumentException => LogLevel.Warning,
            _ => LogLevel.Error
        };
}
