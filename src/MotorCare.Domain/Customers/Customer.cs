using MotorCare.Domain.Common;
using MotorCare.Domain.ValueObjects;

namespace MotorCare.Domain.Customers;

public class Customer : AggregateRoot, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public PhoneNumber? Phone { get; private set; }
    public string? Email { get; private set; }
    public PhoneNumber? Whatsapp { get; private set; }
    public string? Notes { get; private set; }

    private Customer() { } // For EF Core

    public Customer(string tenantId, string fullName, PhoneNumber? phone, string? email, PhoneNumber? whatsapp, string? notes)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("Tenant ID is required.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        FullName = fullName;
        Phone = phone;
        Email = email;
        Whatsapp = whatsapp;
        Notes = notes;
    }

    public void UpdateContactInfo(string fullName, PhoneNumber? phone, string? email, PhoneNumber? whatsapp)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.");
        
        FullName = fullName;
        Phone = phone;
        Email = email;
        Whatsapp = whatsapp;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }
}
