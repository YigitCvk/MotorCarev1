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
    public IReadOnlyCollection<VehicleNote> Notes => _notes;

    private readonly List<VehiclePhoto> _photos = new();
    public IReadOnlyCollection<VehiclePhoto> Photos => _photos;

    private Vehicle() { } // For EF Core

    public Vehicle(string tenantId, PlateNumber plate, string brand, string model, int year)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new DomainException("TenantId is required.");
        if (plate is null) throw new ArgumentNullException(nameof(plate));
        if (string.IsNullOrWhiteSpace(brand)) throw new DomainException("Brand is required.");
        if (string.IsNullOrWhiteSpace(model)) throw new DomainException("Model is required.");
        if (year < 1885 || year > DateTime.UtcNow.Year + 1) throw new DomainException("Invalid vehicle year.");
        
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Plate = plate;
        Brand = brand;
        Model = model;
        Year = year;
    }
    
    public void UpdateDetails(string brand, string model, int year, string? chassisNumber, string? color)
    {
        if (string.IsNullOrWhiteSpace(brand)) throw new DomainException("Brand is required.");
        if (string.IsNullOrWhiteSpace(model)) throw new DomainException("Model is required.");
        if (year < 1885 || year > DateTime.UtcNow.Year + 1) throw new DomainException("Invalid vehicle year.");

        Brand = brand;
        Model = model;
        Year = year;
        ChassisNumber = chassisNumber;
        Color = color;
    }

    public void SetChassisAndColor(string? chassisNumber, string? color)
    {
        ChassisNumber = chassisNumber;
        Color = color;
    }

    public void UpdateMileage(int km)
    {
        if (km < 0) throw new DomainException("Mileage cannot be negative.");
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
        if (string.IsNullOrWhiteSpace(content)) throw new DomainException("Note content is required.");
        _notes.Add(new VehicleNote(content));
    }

    public void AddPhoto(string url, string? description)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new DomainException("Photo URL is required.");
        _photos.Add(new VehiclePhoto(url, description));
    }
}
