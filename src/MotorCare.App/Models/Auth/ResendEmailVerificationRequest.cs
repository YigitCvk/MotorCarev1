using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class ResendEmailVerificationRequest
{
    [Required]
    public string TenantIdentifier { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
