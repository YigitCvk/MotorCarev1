using MotorCare.Domain.Common;

namespace MotorCare.Domain.Tenants;

public class Tenant : AggregateRoot
{
    public string Identifier { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? TaxOffice { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? Website { get; private set; }
    public bool IsActive { get; private set; }

    private Tenant() { } // For EF Core

    public Tenant(string identifier, string name)
    {
        if (string.IsNullOrWhiteSpace(identifier)) throw new DomainException("Identifier is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");

        Id = Guid.NewGuid();
        Identifier = identifier;
        Name = name;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateProfile(
        string name,
        string? legalName,
        string? logoUrl,
        string? phone,
        string? email,
        string? address,
        string? taxOffice,
        string? taxNumber,
        string? website)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");

        Name = name.Trim();
        LegalName = NormalizeOptional(legalName);
        LogoUrl = NormalizeOptional(logoUrl);
        Phone = NormalizeOptional(phone);
        Email = NormalizeOptional(email);
        Address = NormalizeOptional(address);
        TaxOffice = NormalizeOptional(taxOffice);
        TaxNumber = NormalizeOptional(taxNumber);
        Website = NormalizeOptional(website);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
