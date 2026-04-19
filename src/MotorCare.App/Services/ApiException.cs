using System.Net;

namespace MotorCare.App.Services;

public sealed class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string endpoint, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Endpoint = endpoint;
    }

    public HttpStatusCode StatusCode { get; }

    public string Endpoint { get; }
}
