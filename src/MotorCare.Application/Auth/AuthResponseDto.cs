namespace MotorCare.Application.Auth;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string TenantId,
    string TenantIdentifier,
    string Email,
    string Role,
    bool RequiresTwoFactor = false,
    string? TwoFactorToken = null,
    DateTimeOffset? TwoFactorExpiresAtUtc = null,
    string? TwoFactorProvider = null);
