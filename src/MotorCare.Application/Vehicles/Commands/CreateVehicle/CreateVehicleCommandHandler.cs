using MotorCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Domain.Vehicles;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;
using MotorCare.Domain.Common;

namespace MotorCare.Application.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateVehicleCommandHandler> _logger;

    public CreateVehicleCommandHandler(
        IVehicleRepository repository, 
        ITenantProvider tenantProvider,
        ILogger<CreateVehicleCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("Vehicle registration failed: Tenant ID is missing.");
            throw new UnauthorizedAccessException("Tenant ID is required.");
        }

        _logger.LogInformation("Starting vehicle registration for plate {Plate} in tenant {TenantId}.", request.Plate, tenantId);

        var plate = PlateNumber.Create(request.Plate);

        var existing = await _repository.GetByPlateAsync(tenantId, plate.NormalizedValue, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("Vehicle registration failed: Plate {NormalizedPlate} already exists in tenant {TenantId}.", plate.NormalizedValue, tenantId);
            throw new InvalidOperationException("A vehicle with this plate already exists for the tenant.");
        }

        var vehicle = new Vehicle(tenantId, plate, request.Brand, request.Model, request.Year);

        await _repository.AddAsync(vehicle, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully registered vehicle {VehicleId} with plate {NormalizedPlate} in tenant {TenantId}.", vehicle.Id, vehicle.Plate.NormalizedValue, tenantId);
        
        return vehicle.Id;
    }
}
