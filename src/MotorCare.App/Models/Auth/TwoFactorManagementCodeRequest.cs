using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class TwoFactorManagementCodeRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
}
