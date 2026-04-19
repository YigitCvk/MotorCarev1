using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Vehicles.Queries.GetVehiclesByCustomerId;

public sealed class GetVehiclesByCustomerIdQueryHandler : IRequestHandler<GetVehiclesByCustomerIdQuery, IReadOnlyList<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantProvider _tenantProvider;

    public GetVehiclesByCustomerIdQueryHandler(
        IVehicleRepository vehicleRepository,
        ICustomerRepository customerRepository,
        ITenantProvider tenantProvider)
    {
        _vehicleRepository = vehicleRepository;
        _customerRepository = customerRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<VehicleDto>> Handle(GetVehiclesByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId() ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, tenantId, cancellationToken);
        if (customer is null)
        {
            return [];
        }

        var vehicles = await _vehicleRepository.GetByCustomerIdAsync(tenantId, request.CustomerId, cancellationToken);

        return vehicles
            .Select(vehicle => new VehicleDto(
                vehicle.Id,
                vehicle.CurrentCustomerId,
                customer.FullName,
                vehicle.Plate.OriginalValue,
                vehicle.Plate.NormalizedValue,
                vehicle.Brand,
                vehicle.Model,
                vehicle.Year,
                $"{vehicle.Plate.OriginalValue} · {vehicle.Brand} {vehicle.Model}"))
            .ToList();
    }
}
