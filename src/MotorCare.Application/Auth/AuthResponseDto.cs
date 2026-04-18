namespace MotorCare.Application.Auth;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string TenantId,
    string TenantIdentifier,
    string Email,
    string Role);
