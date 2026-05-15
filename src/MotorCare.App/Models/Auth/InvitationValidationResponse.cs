namespace MotorCare.App.Models.Auth;

public sealed class InvitationValidationResponse
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
