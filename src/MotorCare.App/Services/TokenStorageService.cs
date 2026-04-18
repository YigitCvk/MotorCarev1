using Microsoft.JSInterop;

namespace MotorCare.App.Services;

public sealed class TokenStorageService
{
    private const string AccessTokenKey = "motorcare.accessToken";
    private const string RefreshTokenKey = "motorcare.refreshToken";

    private readonly IJSRuntime _jsRuntime;

    public TokenStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask SetTokensAsync(string accessToken, string refreshToken)
    {
        return _jsRuntime.InvokeVoidAsync("motorCareStorage.setTokens", AccessTokenKey, accessToken, RefreshTokenKey, refreshToken);
    }

    public ValueTask<string?> GetAccessTokenAsync()
    {
        return _jsRuntime.InvokeAsync<string?>("motorCareStorage.get", AccessTokenKey);
    }

    public ValueTask<string?> GetRefreshTokenAsync()
    {
        return _jsRuntime.InvokeAsync<string?>("motorCareStorage.get", RefreshTokenKey);
    }

    public ValueTask ClearAsync()
    {
        return _jsRuntime.InvokeVoidAsync("motorCareStorage.clearTokens", AccessTokenKey, RefreshTokenKey);
    }
}
