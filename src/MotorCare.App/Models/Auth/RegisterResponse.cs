namespace MotorCare.App.Models.Auth;

public sealed class RegisterResponse
{
    public Guid TenantId { get; set; }
    public string TenantIdentifier { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public bool VerificationEmailSent { get; set; }
}
