using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderById;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrders;

public class GetServiceOrdersQueryHandler : IRequestHandler<GetServiceOrdersQuery, PagedResult<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITenantProvider _tenantProvider;

    public GetServiceOrdersQueryHandler(
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

    public async Task<PagedResult<ServiceOrderDto>> Handle(GetServiceOrdersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var pagination = PaginationRequest.Of(request.PageNumber, request.PageSize);

        var (orders, totalCount) = await _repository.GetFilteredPagedAsync(
            tenantId,
            request.CustomerId,
            request.Status,
            request.SearchText,
            request.OpenedFrom,
            request.OpenedTo,
            pagination.SafePageNumber,
            pagination.SafePageSize,
            cancellationToken);

        // Batch-fetch customers and vehicles with two queries instead of 2×N individual lookups.
        var customerIds = orders.Select(o => o.CustomerId).Distinct();
        var vehicleIds = orders.Select(o => o.VehicleId).Distinct();

        var customers = await _customerRepository.GetByIdsAsync(customerIds, tenantId, cancellationToken);
        var vehicles = await _vehicleRepository.GetByIdsAsync(vehicleIds, tenantId, cancellationToken);

        var items = orders.Select(order =>
        {
            customers.TryGetValue(order.CustomerId, out var customer);
            vehicles.TryGetValue(order.VehicleId, out var vehicle);

            return new ServiceOrderDto(
                order.Id,
                order.OrderNo,
                order.VehicleId,
                order.CustomerId,
                customer?.FullName,
                vehicle?.Plate.OriginalValue,
                vehicle is null ? null : $"{vehicle.Brand} {vehicle.Model} ({vehicle.Year})",
                order.Status.ToString(),
                order.OpenedAt,
                order.ClosedAt,
                order.VehicleKm,
                order.Complaint,
                order.WorkDescription,
                order.InternalNote,
                order.LaborTotal,
                order.PartsTotal,
                order.DiscountTotal,
                order.GrandTotal,
                order.PaidTotal,
                order.RemainingTotal,
                [],
                [],
                []);
        }).ToList();

        return PagedResult<ServiceOrderDto>.Create(items, pagination.SafePageNumber, pagination.SafePageSize, totalCount);
    }
}
