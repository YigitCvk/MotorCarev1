namespace MotorCare.App.Models.Auth;

public sealed class SecurityStatusResponse
{
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorProvider { get; set; }
}
