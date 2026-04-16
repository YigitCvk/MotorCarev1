using MediatR;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;
using MotorCare.Domain.Common;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

public class GetVehicleByPlateQueryHandler : IRequestHandler<GetVehicleByPlateQuery, VehicleDto?>
{
    private readonly IVehicleRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetVehicleByPlateQueryHandler(IVehicleRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<VehicleDto?> Handle(GetVehicleByPlateQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrEmpty(tenantId)) throw new UnauthorizedAccessException("Tenant ID is required.");

        var normalizedPlate = PlateNumber.Create(request.Plate).NormalizedValue;

        var vehicle = await _repository.GetByPlateAsync(tenantId, normalizedPlate, cancellationToken);
        if (vehicle == null) return null;

        return new VehicleDto(
            vehicle.Id,
            vehicle.Plate.OriginalValue,
            vehicle.Plate.NormalizedValue,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year);
    }
}
