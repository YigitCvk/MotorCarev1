using MotorCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;
using MotorCare.Domain.Common;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

public class GetVehicleByPlateQueryHandler : IRequestHandler<GetVehicleByPlateQuery, VehicleDto?>
{
    private readonly IVehicleRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<GetVehicleByPlateQueryHandler> _logger;

    public GetVehicleByPlateQueryHandler(
        IVehicleRepository repository, 
        ITenantProvider tenantProvider,
        ILogger<GetVehicleByPlateQueryHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<VehicleDto?> Handle(GetVehicleByPlateQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("Vehicle lookup failed: Tenant ID is missing.");
            throw new UnauthorizedAccessException("Tenant ID is required.");
        }

        _logger.LogInformation("Starting vehicle lookup for plate {Plate} in tenant {TenantId}.", request.Plate, tenantId);

        var normalizedPlate = PlateNumber.Create(request.Plate).NormalizedValue;

        var vehicle = await _repository.GetByPlateAsync(tenantId, normalizedPlate, cancellationToken);
        if (vehicle == null)
        {
            _logger.LogInformation("Vehicle lookup not-found: Plate {NormalizedPlate} does not exist in tenant {TenantId}.", normalizedPlate, tenantId);
            return null;
        }

        _logger.LogInformation("Successfully found vehicle {VehicleId} with plate {NormalizedPlate} in tenant {TenantId}.", vehicle.Id, vehicle.Plate.NormalizedValue, tenantId);

        return new VehicleDto(
            vehicle.Id,
            vehicle.Plate.OriginalValue,
            vehicle.Plate.NormalizedValue,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year);
    }
}
