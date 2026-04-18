using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderById;

public class GetServiceOrderByIdQueryHandler : IRequestHandler<GetServiceOrderByIdQuery, ServiceOrderDto?>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetServiceOrderByIdQueryHandler(IServiceOrderRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<ServiceOrderDto?> Handle(GetServiceOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        return new ServiceOrderDto(
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
            order.Operations
                .Select(operation => new ServiceOperationItemDto(operation.Id, operation.Description, operation.Price))
                .ToList(),
            order.Parts
                .Select(part => new ServicePartItemDto(part.Id, part.PartName, part.PartNumber, part.UnitPrice, part.Quantity, part.TotalPrice))
                .ToList(),
            order.Payments
                .Select(payment => new ServicePaymentDto(payment.Id, payment.Amount, payment.Method.ToString(), payment.PaymentDate))
                .ToList());
    }
}
