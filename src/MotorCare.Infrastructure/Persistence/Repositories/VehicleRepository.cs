using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Vehicles;
using MotorCare.Domain.Repositories;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _context;

    public VehicleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Vehicle>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Vehicle?> GetByPlateAsync(string tenantId, string normalizedPlate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Vehicle>()
            .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.Plate.NormalizedValue == normalizedPlate, cancellationToken);
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        await _context.Set<Vehicle>().AddAsync(vehicle, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
