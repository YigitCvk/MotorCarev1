using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class ResendTwoFactorRequest
{
    [Required]
    public string Ticket { get; set; } = string.Empty;
}
