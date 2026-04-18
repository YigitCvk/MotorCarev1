using MotorCare.Domain.Vehicles;

namespace MotorCare.Domain.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByPlateAsync(string tenantId, string normalizedPlate, CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    void Update(Vehicle vehicle);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
