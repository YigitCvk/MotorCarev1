using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
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
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly ILogger<CreateServiceOrderCommandHandler> _logger;

    public CreateServiceOrderCommandHandler(
        IServiceOrderRepository serviceOrderRepository,
        IVehicleRepository vehicleRepository,
        ICustomerRepository customerRepository,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider,
        IOrderNumberGenerator orderNumberGenerator,
        ILogger<CreateServiceOrderCommandHandler> logger)
    {
        _serviceOrderRepository = serviceOrderRepository;
        _vehicleRepository = vehicleRepository;
        _customerRepository = customerRepository;
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
        _orderNumberGenerator = orderNumberGenerator;
        _logger = logger;
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

        if (request.Consumables is not null)
        {
            foreach (var consumable in request.Consumables
                         .Where(x => !string.IsNullOrWhiteSpace(x.Category) && !string.IsNullOrWhiteSpace(x.ProductName)))
            {
                order.AddConsumable(
                    consumable.Category,
                    consumable.ProductName,
                    consumable.Brand,
                    consumable.SubCategory,
                    consumable.Specification,
                    consumable.Notes);
            }
        }

        await _serviceOrderRepository.AddAsync(order, cancellationToken);
        await _serviceOrderRepository.AddStatusHistoryAsync(
            new ServiceOrderStatusHistory(
                tenantId,
                order.Id,
                null,
                order.Status,
                "Servis emri oluşturuldu.",
                _currentUserProvider.GetUserId(),
                _currentUserProvider.GetEmail(),
                DateTimeOffset.UtcNow),
            cancellationToken);
        await _serviceOrderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.ServiceOrderCreated,
            "Service order created. ServiceOrderId={ServiceOrderId} OrderNo={OrderNo} CustomerId={CustomerId} VehicleId={VehicleId}",
            order.Id,
            orderNo,
            request.CustomerId,
            request.VehicleId);

        if (order.Consumables.Count > 0)
        {
            _logger.LogInformation(
                EventIdStore.ServiceOrder.ConsumablesAttachedToServiceOrder,
                "Consumables attached to service order. ServiceOrderId={ServiceOrderId} ConsumableCount={ConsumableCount}",
                order.Id,
                order.Consumables.Count);
        }

        return order.Id;
    }
}
