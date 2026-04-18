using System.ComponentModel.DataAnnotations;

namespace MotorCare.App.Models.Auth;

public sealed class RegisterRequest
{
    [Required]
    [MaxLength(50)]
    public string TenantIdentifier { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string TenantName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string OwnerFullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string OwnerEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string OwnerPassword { get; set; } = string.Empty;
}
