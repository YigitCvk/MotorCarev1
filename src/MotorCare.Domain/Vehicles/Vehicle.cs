using MotorCare.Domain.Common;
using MotorCare.Domain.ValueObjects;
using MotorCare.Domain.Vehicles.Entities;

namespace MotorCare.Domain.Vehicles;

public class Vehicle : AggregateRoot, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public PlateNumber Plate { get; private set; } = null!;
    public string? ChassisNumber { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public int? CurrentKm { get; private set; }
    public string? Color { get; private set; }
    public Guid? CurrentCustomerId { get; private set; }

    private readonly List<VehicleNote> _notes = new();
    public IReadOnlyCollection<VehicleNote> Notes => _notes.AsReadOnly();

    private readonly List<VehiclePhoto> _photos = new();
    public IReadOnlyCollection<VehiclePhoto> Photos => _photos.AsReadOnly();

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
    
    public void UpdateDetails(string brand, string model, int year, string? chassisNumber, string? color)
    {
        Brand = brand;
        Model = model;
        Year = year;
        ChassisNumber = chassisNumber;
        Color = color;
    }

    public void UpdateMileage(int km)
    {
        if (km < 0) throw new ArgumentException("Mileage cannot be negative.");
        // Usually, km just goes up, but sometimes corrections are needed.
        CurrentKm = km;
    }

    public void AssignCustomer(Guid customerId)
    {
        CurrentCustomerId = customerId;
    }

    public void UnassignCustomer()
    {
        CurrentCustomerId = null;
    }

    public void AddNote(string content)
    {
        _notes.Add(new VehicleNote(content));
    }

    public void AddPhoto(string url, string? description)
    {
        _photos.Add(new VehiclePhoto(url, description));
    }
}
