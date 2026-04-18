using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class LoginRequest
{
    [Required]
    public string TenantIdentifier { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
