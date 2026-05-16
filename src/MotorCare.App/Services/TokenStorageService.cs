using Microsoft.JSInterop;

namespace MotorCare.App.Services;

public sealed class TokenStorageService
{
    public const string AccessTokenKey = "motorcare.accessToken";
    private const string RefreshTokenKey = "motorcare.refreshToken";

    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<TokenStorageService> _logger;

    public TokenStorageService(IJSRuntime jsRuntime, ILogger<TokenStorageService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async ValueTask SetTokensAsync(string accessToken, string refreshToken)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("motorCareStorage.setTokens", AccessTokenKey, accessToken, RefreshTokenKey, refreshToken);
        }
        catch (Exception ex) when (IsRecoverableStorageException(ex))
        {
            _logger.LogDebug("Browser storage set skipped because the circuit is disconnected. ExceptionType={ExceptionType}", ex.GetType().Name);
        }
    }

    public async ValueTask<string?> GetAccessTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("motorCareStorage.get", AccessTokenKey);
        }
        catch (Exception ex) when (IsRecoverableStorageException(ex))
        {
            _logger.LogDebug("Browser storage read skipped because the circuit is disconnected. ExceptionType={ExceptionType}", ex.GetType().Name);
            return null;
        }
    }

    public async ValueTask<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("motorCareStorage.get", RefreshTokenKey);
        }
        catch (Exception ex) when (IsRecoverableStorageException(ex))
        {
            _logger.LogDebug("Browser storage read skipped because the circuit is disconnected. ExceptionType={ExceptionType}", ex.GetType().Name);
            return null;
        }
    }

    public async ValueTask ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("motorCareStorage.clearTokens", AccessTokenKey, RefreshTokenKey);
        }
        catch (Exception ex) when (IsRecoverableStorageException(ex))
        {
            _logger.LogDebug("Browser storage clear skipped because the circuit is disconnected. ExceptionType={ExceptionType}", ex.GetType().Name);
        }
    }

    private static bool IsRecoverableStorageException(Exception exception)
        => exception is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException or OperationCanceledException or ObjectDisposedException;
}
