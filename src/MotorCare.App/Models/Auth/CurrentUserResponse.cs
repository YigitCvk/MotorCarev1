namespace MotorCare.App.Models.Auth;

public sealed class CurrentUserResponse
{
    public Guid UserId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string TenantIdentifier { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
