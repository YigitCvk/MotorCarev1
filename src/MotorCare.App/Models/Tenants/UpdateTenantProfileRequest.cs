namespace MotorCare.App.Models.Tenants;

public sealed class UpdateTenantProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public string? Website { get; set; }
}
