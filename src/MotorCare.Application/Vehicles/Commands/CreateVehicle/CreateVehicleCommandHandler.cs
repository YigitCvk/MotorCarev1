using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;
using MotorCare.Domain.Vehicles;

namespace MotorCare.Application.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateVehicleCommandHandler> _logger;

    public CreateVehicleCommandHandler(
        IVehicleRepository repository,
        ICustomerRepository customerRepository,
        ITenantProvider tenantProvider,
        ILogger<CreateVehicleCommandHandler> logger)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        _logger.LogInformation(
            EventIdStore.Vehicle.VehicleCreated,
            "Starting vehicle registration for plate {Plate} in tenant {TenantId}.",
            request.Plate,
            tenantId);

        var plate = PlateNumber.Create(request.Plate);

        var existing = await _repository.GetByPlateAsync(tenantId, plate.NormalizedValue, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning(
                EventIdStore.Vehicle.VehicleCreated,
                "Vehicle registration failed: Plate {NormalizedPlate} already exists in tenant {TenantId}.",
                plate.NormalizedValue,
                tenantId);
            throw new ConflictException("A vehicle with this plate already exists for the tenant.");
        }

        var vehicle = new Vehicle(tenantId, plate, request.Brand, request.Model, request.Year);

        vehicle.SetTechnicalDetails(request.ChassisNumber, request.EngineNumber, request.Color);

        if (request.CurrentKm.HasValue)
        {
            vehicle.UpdateMileage(request.CurrentKm.Value);
        }

        if (request.CurrentCustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CurrentCustomerId.Value, tenantId, cancellationToken);
            if (customer is null)
            {
                throw new AppValidationException(
                [
                    new ValidationFailure(nameof(request.CurrentCustomerId), "Seçilen müşteri bu işletme altinda bulunamadı.")
                ]);
            }

            vehicle.AssignCustomer(request.CurrentCustomerId.Value);
        }

        await _repository.AddAsync(vehicle, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Vehicle.VehicleCreated,
            "Successfully registered vehicle {VehicleId} with plate {NormalizedPlate} in tenant {TenantId}. CustomerId={CustomerId}",
            vehicle.Id,
            vehicle.Plate.NormalizedValue,
            tenantId,
            vehicle.CurrentCustomerId);

        return vehicle.Id;
    }
}
