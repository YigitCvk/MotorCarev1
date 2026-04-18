namespace MotorCare.Application.Auth;

public sealed record CurrentUserDto(
    Guid UserId,
    string TenantId,
    string TenantIdentifier,
    string FullName,
    string Email,
    string Role);
