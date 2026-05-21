using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
