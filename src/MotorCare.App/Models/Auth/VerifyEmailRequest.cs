using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class VerifyEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
