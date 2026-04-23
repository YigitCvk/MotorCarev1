using MotorCare.App.Models.Auth;
using MotorCare.App.Models.Dashboard;

namespace MotorCare.App.Services;

public sealed class AuthService
{
    private readonly ApiClient _apiClient;
    private readonly TokenStorageService _tokenStorageService;

    public AuthService(ApiClient apiClient, TokenStorageService tokenStorageService)
    {
        _apiClient = apiClient;
        _tokenStorageService = tokenStorageService;
    }

    public event Action? AuthenticationStateChanged;

    public Task<RegisterResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", request, authorized: false, cancellationToken);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("/api/auth/login", request, authorized: false, cancellationToken);
        if (response is not null)
        {
            await _tokenStorageService.SetTokensAsync(response.AccessToken, response.RefreshToken);
            AuthenticationStateChanged?.Invoke();
        }

        return response;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            try
            {
                await _apiClient.PostAsync("/api/auth/logout", new LogoutRequest { RefreshToken = refreshToken }, authorized: true, cancellationToken);
            }
            catch
            {
                // Ignore logout API failure for local MVP and clear local tokens.
            }
        }

        await _tokenStorageService.ClearAsync();
        AuthenticationStateChanged?.Invoke();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _tokenStorageService.GetAccessTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<CurrentUserResponse>("/api/auth/me", authorized: true, cancellationToken);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _tokenStorageService.ClearAsync();
            AuthenticationStateChanged?.Invoke();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public Task<DailySummaryResponse?> GetDailySummaryAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<DailySummaryResponse>("/api/dashboard/daily", authorized: true, cancellationToken);
    }
}
