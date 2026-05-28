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

    public async Task<Vehicle?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .AsSplitQuery()
            .Include(v => v.Notes)
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId, cancellationToken);
    }

    public async Task<Vehicle?> GetByPlateAsync(string tenantId, string normalizedPlate, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.Plate.NormalizedValue == normalizedPlate, cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetByCustomerIdAsync(string tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .AsSplitQuery()
            .Where(v => v.TenantId == tenantId && v.CurrentCustomerId == customerId)
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ThenBy(v => v.Plate.NormalizedValue)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, Vehicle>> GetByIdsAsync(IEnumerable<Guid> ids, string tenantId, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        return await _context.Vehicles
            .AsNoTracking()
            .AsSplitQuery()
            .Where(v => v.TenantId == tenantId && idList.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        await _context.Vehicles.AddAsync(vehicle, cancellationToken);
    }

    public void Update(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
