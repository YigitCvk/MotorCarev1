using System.Net;

namespace MotorCare.App.Services;

public sealed class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string endpoint, string message, string? code = null)
        : base(message)
    {
        StatusCode = statusCode;
        Endpoint = endpoint;
        Code = code;
    }

    public HttpStatusCode StatusCode { get; }
    public string Endpoint { get; }

    /// <summary>
    /// Machine-readable error code from the API (e.g. "LOGIN_FAILED", "EMAIL_NOT_VERIFIED").
    /// Null if the response did not include a code field.
    /// </summary>
    public string? Code { get; }
}
