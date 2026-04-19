using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderById;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrders;

public class GetServiceOrdersQueryHandler : IRequestHandler<GetServiceOrdersQuery, PagedResult<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetServiceOrdersQueryHandler(IServiceOrderRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
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

        var items = orders.Select(order => new ServiceOrderDto(
                order.Id,
                order.OrderNo,
                order.VehicleId,
                order.CustomerId,
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
                []))
            .ToList();

        return PagedResult<ServiceOrderDto>.Create(items, pagination.SafePageNumber, pagination.SafePageSize, totalCount);
    }
}
