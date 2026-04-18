namespace MotorCare.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public int? StatusCode { get; }

    private Result(bool isSuccess, T? data, string? error, int? statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T data) => new(true, data, null, null);
    public static Result<T> Failure(string error, int statusCode = 400) => new(false, default, error, statusCode);
}
