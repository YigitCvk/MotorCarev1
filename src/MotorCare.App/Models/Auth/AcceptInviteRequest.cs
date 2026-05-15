using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class AcceptInviteRequest
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required] public string FullName { get; set; } = string.Empty;
    [Required][MinLength(8)] public string Password { get; set; } = string.Empty;
    [Required] public string ConfirmPassword { get; set; } = string.Empty;
}
