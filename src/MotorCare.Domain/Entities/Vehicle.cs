using MotorCare.Domain.ValueObjects;

namespace MotorCare.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public PlateNumber Plate { get; private set; } = null!;
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    
    private Vehicle() { } // For EF Core

    public Vehicle(string tenantId, PlateNumber plate, string brand, string model, int year)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("TenantId missing");
        
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Plate = plate ?? throw new ArgumentNullException(nameof(plate));
        Brand = brand;
        Model = model;
        Year = year;
    }
    
    public void UpdateDetails(string brand, string model, int year)
    {
        Brand = brand;
        Model = model;
        Year = year;
    }
}
