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

        var items = new List<ServiceOrderDto>(orders.Count);
        foreach (var order in orders)
        {
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId, tenantId, cancellationToken);
            var vehicle = await _vehicleRepository.GetByIdAsync(order.VehicleId, tenantId, cancellationToken);

            items.Add(new ServiceOrderDto(
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
                []));
        }

        return PagedResult<ServiceOrderDto>.Create(items, pagination.SafePageNumber, pagination.SafePageSize, totalCount);
    }
}
