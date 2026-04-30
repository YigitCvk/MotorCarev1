using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class VerifyTwoFactorRequest
{
    [Required]
    public string Ticket { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    [MaxLength(8)]
    public string Code { get; set; } = string.Empty;
}
