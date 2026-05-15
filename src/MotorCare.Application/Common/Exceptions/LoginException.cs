namespace MotorCare.Application.Common.Exceptions;

/// <summary>
/// Thrown for all login-specific failures. Carries a stable error code and a
/// Turkish user-facing message that is safe to display. The base exception
/// message is used for logging only and may differ from <see cref="UserMessage"/>.
/// </summary>
public sealed class LoginException : Exception
{
    public string Code { get; }
    public string UserMessage { get; }

    public LoginException(string code, string userMessage, string? logMessage = null)
        : base(logMessage ?? userMessage)
    {
        Code = code;
        UserMessage = userMessage;
    }
}
