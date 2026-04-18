using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.ServiceOrders.Commands.CreateServiceOrder;

public class CreateServiceOrderCommandHandler : IRequestHandler<CreateServiceOrderCommand, Guid>
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IOrderNumberGenerator _orderNumberGenerator;

    public CreateServiceOrderCommandHandler(
        IServiceOrderRepository serviceOrderRepository,
        IVehicleRepository vehicleRepository,
        ICustomerRepository customerRepository,
        ITenantProvider tenantProvider,
        IOrderNumberGenerator orderNumberGenerator)
    {
        _serviceOrderRepository = serviceOrderRepository;
        _vehicleRepository = vehicleRepository;
        _customerRepository = customerRepository;
        _tenantProvider = tenantProvider;
        _orderNumberGenerator = orderNumberGenerator;
    }

    public async Task<Guid> Handle(CreateServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Vehicles.Vehicle), request.VehicleId);

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Customers.Customer), request.CustomerId);

        var orderNo = await _orderNumberGenerator.GenerateAsync(tenantId, cancellationToken);

        var existing = await _serviceOrderRepository.GetByOrderNoAsync(tenantId, orderNo, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"A service order with order number '{orderNo}' already exists.");
        }

        var order = new ServiceOrder(
            tenantId,
            orderNo,
            vehicle.Id,
            customer.Id,
            request.VehicleKm,
            request.Complaint);

        await _serviceOrderRepository.AddAsync(order, cancellationToken);
        await _serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
