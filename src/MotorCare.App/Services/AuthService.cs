using MotorCare.Application.Common;
using MotorCare.App.Models.Auth;
using MotorCare.App.Models.Dashboard;
using Microsoft.JSInterop;

namespace MotorCare.App.Services;

public sealed class AuthService
{
    private readonly ApiClient _apiClient;
    private readonly TokenStorageService _tokenStorageService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ApiClient apiClient, TokenStorageService tokenStorageService, ILogger<AuthService> logger)
    {
        _apiClient = apiClient;
        _tokenStorageService = tokenStorageService;
        _logger = logger;
    }

    public event Action? AuthenticationStateChanged;

    public Task<RegisterResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", request, authorized: false, cancellationToken);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("/api/auth/login", request, authorized: false, cancellationToken);
        if (response is not null &&
            !response.RequiresTwoFactor &&
            !string.IsNullOrWhiteSpace(response.AccessToken) &&
            !string.IsNullOrWhiteSpace(response.RefreshToken))
        {
            await _tokenStorageService.SetTokensAsync(response.AccessToken, response.RefreshToken);
            AuthenticationStateChanged?.Invoke();
        }

        return response;
    }

    public Task<AuthActionResponse?> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<ForgotPasswordRequest, AuthActionResponse>("/api/auth/forgot-password", request, authorized: false, cancellationToken);
    }

    public Task<AuthActionResponse?> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<ResetPasswordRequest, AuthActionResponse>("/api/auth/reset-password", request, authorized: false, cancellationToken);
    }

    public Task<AuthActionResponse?> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<VerifyEmailRequest, AuthActionResponse>("/api/auth/verify-email-code", request, authorized: false, cancellationToken);
    }

    public Task<AuthActionResponse?> ResendEmailVerificationAsync(ResendEmailVerificationRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<ResendEmailVerificationRequest, AuthActionResponse>("/api/auth/resend-email-verification-code", request, authorized: false, cancellationToken);
    }

    public Task<AuthActionResponse?> AcceptInviteAsync(AcceptInviteRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<AcceptInviteRequest, AuthActionResponse>("/api/auth/accept-invite", request, authorized: false, cancellationToken);
    }

    public Task<InvitationValidationResponse?> ValidateInviteAsync(string token, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<InvitationValidationResponse>(
            $"/api/users/invitations/{Uri.EscapeDataString(token)}/validate",
            authorized: false,
            cancellationToken);
    }

    public async Task<LoginResponse?> VerifyTwoFactorAsync(VerifyTwoFactorRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<VerifyTwoFactorRequest, LoginResponse>("/api/auth/two-factor/verify", request, authorized: false, cancellationToken);
        if (response is not null &&
            !string.IsNullOrWhiteSpace(response.AccessToken) &&
            !string.IsNullOrWhiteSpace(response.RefreshToken))
        {
            await _tokenStorageService.SetTokensAsync(response.AccessToken, response.RefreshToken);
            AuthenticationStateChanged?.Invoke();
        }

        return response;
    }

    public Task<AuthActionResponse?> ResendTwoFactorAsync(ResendTwoFactorRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<ResendTwoFactorRequest, AuthActionResponse>("/api/auth/two-factor/resend", request, authorized: false, cancellationToken);
    }

    public Task<SecurityStatusResponse?> GetSecurityStatusAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<SecurityStatusResponse>("/api/auth/security-status", authorized: true, cancellationToken);
    }

    public Task<AuthActionResponse?> SendEnableTwoFactorCodeAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<object, AuthActionResponse>("/api/auth/2fa/enable/send-code", new { }, authorized: true, cancellationToken);
    }

    public Task<AuthActionResponse?> ConfirmEnableTwoFactorAsync(TwoFactorManagementCodeRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<TwoFactorManagementCodeRequest, AuthActionResponse>("/api/auth/2fa/enable/confirm", request, authorized: true, cancellationToken);
    }

    public Task<AuthActionResponse?> SendDisableTwoFactorCodeAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<object, AuthActionResponse>("/api/auth/2fa/disable/send-code", new { }, authorized: true, cancellationToken);
    }

    public Task<AuthActionResponse?> ConfirmDisableTwoFactorAsync(TwoFactorManagementCodeRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<TwoFactorManagementCodeRequest, AuthActionResponse>("/api/auth/2fa/disable/confirm", request, authorized: true, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        string? refreshToken = null;

        try
        {
            refreshToken = await _tokenStorageService.GetRefreshTokenAsync();
        }
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or ObjectDisposedException or TaskCanceledException or OperationCanceledException)
        {
            _logger.LogDebug("Logout browser storage read skipped because circuit is disconnected. ExceptionType={ExceptionType}", ex.GetType().Name);
        }

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
        try
        {
            var token = await _tokenStorageService.GetAccessTokenAsync();
            return !string.IsNullOrWhiteSpace(token);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or ObjectDisposedException or TaskCanceledException or OperationCanceledException)
        {
            _logger.LogDebug(EventIdStore.Common.AuthRecoverySkippedDueToDisposedCircuit,
                "Authentication check skipped because circuit is disconnected. ExceptionType={ExceptionType}",
                ex.GetType().Name);
            return false;
        }
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
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or ObjectDisposedException or TaskCanceledException or OperationCanceledException)
        {
            _logger.LogDebug("Current user lookup skipped because circuit is disconnected. ExceptionType={ExceptionType}", ex.GetType().Name);
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
