using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Queries.GetOpenBalances;

public class GetOpenBalancesQueryHandler : IRequestHandler<GetOpenBalancesQuery, IReadOnlyList<OpenBalanceDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITenantProvider _tenantProvider;

    public GetOpenBalancesQueryHandler(
        IServiceOrderRepository repository,
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        ITenantProvider tenantProvider)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<OpenBalanceDto>> Handle(GetOpenBalancesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var allOrders = await _repository.GetFilteredAsync(
            tenantId,
            customerId: null,
            status: null,
            searchText: null,
            openedFrom: null,
            openedTo: null,
            cancellationToken);

        // Filter to orders with outstanding balance, excluding cancelled
        var openBalanceOrders = allOrders
            .Where(o => o.Status != ServiceOrderStatus.Cancelled && o.RemainingTotal > 0)
            .OrderByDescending(o => o.RemainingTotal)
            .Take(request.Take)
            .ToList();

        if (openBalanceOrders.Count == 0)
            return [];

        // Batch-fetch customers and vehicles to avoid N+1 queries
        var customerIds = openBalanceOrders.Select(o => o.CustomerId).Distinct();
        var vehicleIds = openBalanceOrders.Select(o => o.VehicleId).Distinct();

        var customers = await _customerRepository.GetByIdsAsync(customerIds, tenantId, cancellationToken);
        var vehicles = await _vehicleRepository.GetByIdsAsync(vehicleIds, tenantId, cancellationToken);

        return openBalanceOrders.Select(order =>
        {
            customers.TryGetValue(order.CustomerId, out var customer);
            vehicles.TryGetValue(order.VehicleId, out var vehicle);

            return new OpenBalanceDto(
                order.Id,
                order.OrderNo,
                customer?.FullName,
                vehicle?.Plate.OriginalValue,
                order.GrandTotal,
                order.PaidTotal,
                order.RemainingTotal,
                order.Status.ToString(),
                order.OpenedAt);
        }).ToList();
    }
}
