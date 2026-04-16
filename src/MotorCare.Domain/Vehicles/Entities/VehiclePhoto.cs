using MotorCare.Domain.Common;

namespace MotorCare.Domain.Vehicles.Entities;

public class VehiclePhoto : AuditableEntity
{
    public string PhotoUrl { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private VehiclePhoto() { }

    internal VehiclePhoto(string photoUrl, string? description)
    {
        if (string.IsNullOrWhiteSpace(photoUrl)) throw new DomainException("Photo URL is required.");

        Id = Guid.NewGuid();
        PhotoUrl = photoUrl;
        Description = description;
    }
}
