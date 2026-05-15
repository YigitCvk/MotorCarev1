using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class VerifyEmailRequest
{
    [Required]
    public string TenantIdentifier { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Doğrulama kodu 6 haneli rakamdan oluşmalıdır.")]
    public string Code { get; set; } = string.Empty;
}
