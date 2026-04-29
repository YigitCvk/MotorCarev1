using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

public class GetVehicleByPlateQueryHandler : IRequestHandler<GetVehicleByPlateQuery, VehicleDto?>
{
    private readonly IVehicleRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<GetVehicleByPlateQueryHandler> _logger;

    public GetVehicleByPlateQueryHandler(
        IVehicleRepository repository,
        ICustomerRepository customerRepository,
        ITenantProvider tenantProvider,
        ILogger<GetVehicleByPlateQueryHandler> logger)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<VehicleDto?> Handle(GetVehicleByPlateQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning(
                EventIdStore.Vehicle.VehicleLookupByPlate,
                "Vehicle lookup failed: Tenant ID is missing.");
            throw new UnauthorizedAccessException("Tenant ID is required.");
        }

        _logger.LogInformation(
            EventIdStore.Vehicle.VehicleLookupByPlate,
            "Starting vehicle lookup for plate {Plate} in tenant {TenantId}.",
            request.Plate,
            tenantId);

        var normalizedPlate = PlateNumber.Create(request.Plate).NormalizedValue;
        var vehicle = await _repository.GetByPlateAsync(tenantId, normalizedPlate, cancellationToken);
        if (vehicle is null)
        {
            _logger.LogInformation(
                EventIdStore.Vehicle.VehicleLookupByPlate,
                "Vehicle lookup not-found: Plate {NormalizedPlate} does not exist in tenant {TenantId}.",
                normalizedPlate,
                tenantId);
            return null;
        }

        _logger.LogInformation(
            EventIdStore.Vehicle.VehicleFetched,
            "Successfully found vehicle {VehicleId} with plate {NormalizedPlate} in tenant {TenantId}. CustomerId={CustomerId}",
            vehicle.Id,
            vehicle.Plate.NormalizedValue,
            tenantId,
            vehicle.CurrentCustomerId);

        string? customerName = null;
        if (vehicle.CurrentCustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(vehicle.CurrentCustomerId.Value, tenantId, cancellationToken);
            customerName = customer?.FullName;
        }

        return new VehicleDto(
            vehicle.Id,
            vehicle.CurrentCustomerId,
            customerName,
            vehicle.Plate.OriginalValue,
            vehicle.Plate.NormalizedValue,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            $"{vehicle.Plate.OriginalValue} - {vehicle.Brand} {vehicle.Model}",
            vehicle.ChassisNumber,
            vehicle.EngineNumber,
            vehicle.CurrentKm);
    }
}
