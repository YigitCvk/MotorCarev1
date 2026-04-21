using MotorCare.Domain.Enums;
using MotorCare.Domain.Inspections;

namespace MotorCare.Domain.Repositories;

public interface IMotorcycleInspectionRepository
{
    Task<MotorcycleInspection?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<MotorcycleInspection?> GetByInspectionNoAsync(string tenantId, string inspectionNo, CancellationToken cancellationToken = default);
    Task<int> GetTodayCountAsync(string tenantId, DateTimeOffset dayStartUtc, DateTimeOffset dayEndUtc, CancellationToken cancellationToken = default);
    Task<(List<MotorcycleInspection> Items, int TotalCount)> GetPagedAsync(
        string tenantId,
        string? searchText,
        MotorcycleInspectionPackageType? packageType,
        MotorcycleInspectionStatus? status,
        Guid? customerId,
        Guid? vehicleId,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(MotorcycleInspection inspection, CancellationToken cancellationToken = default);
    void Update(MotorcycleInspection inspection);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
