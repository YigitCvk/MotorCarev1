using MediatR;
using MotorCare.Domain.Entities;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;
using MotorCare.Domain.Common;

namespace MotorCare.Application.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public CreateVehicleCommandHandler(IVehicleRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new UnauthorizedAccessException("Tenant ID is required.");
        }

        var plate = PlateNumber.Create(request.Plate);

        var existing = await _repository.GetByPlateAsync(tenantId, plate.NormalizedValue, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException("A vehicle with this plate already exists for the tenant.");
        }

        var vehicle = new Vehicle(tenantId, plate, request.Brand, request.Model, request.Year);

        await _repository.AddAsync(vehicle, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        
        return vehicle.Id;
    }
}
