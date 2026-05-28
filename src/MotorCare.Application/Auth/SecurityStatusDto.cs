namespace MotorCare.Application.Auth;

public sealed record SecurityStatusDto(
    string Email,
    bool IsEmailVerified,
    bool TwoFactorEnabled,
    string? TwoFactorProvider);
