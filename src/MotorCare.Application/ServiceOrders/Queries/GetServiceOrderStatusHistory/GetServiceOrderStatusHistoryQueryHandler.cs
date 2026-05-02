using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderStatusHistory;

public sealed class GetServiceOrderStatusHistoryQueryHandler
    : IRequestHandler<GetServiceOrderStatusHistoryQuery, IReadOnlyList<ServiceOrderStatusHistoryDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<GetServiceOrderStatusHistoryQueryHandler> _logger;

    public GetServiceOrderStatusHistoryQueryHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ILogger<GetServiceOrderStatusHistoryQueryHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ServiceOrderStatusHistoryDto>> Handle(
        GetServiceOrderStatusHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.ServiceOrderId, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.ServiceOrderId);

        var history = await _repository.GetStatusHistoryAsync(order.Id, tenantId, cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.ServiceOrderStatusHistoryFetched,
            "Service order status history fetched. ServiceOrderId={ServiceOrderId} TenantId={TenantId} HistoryCount={HistoryCount}",
            order.Id,
            tenantId,
            history.Count);

        return history
            .Select(x => new ServiceOrderStatusHistoryDto(
                x.Id,
                x.FromStatus?.ToString(),
                x.FromStatus.HasValue ? ToStatusText(x.FromStatus.Value) : null,
                x.ToStatus.ToString(),
                ToStatusText(x.ToStatus),
                x.Note,
                x.ChangedByUserName,
                x.CreatedAt))
            .ToList();
    }

    private static string ToStatusText(ServiceOrderStatus status) => status switch
    {
        ServiceOrderStatus.Open => "Açık",
        ServiceOrderStatus.InProgress => "İşlemde",
        ServiceOrderStatus.WaitingForParts => "Parça Bekliyor",
        ServiceOrderStatus.Completed => "Teslime Hazır",
        ServiceOrderStatus.Delivered => "Teslim Edildi",
        ServiceOrderStatus.Cancelled => "İptal Edildi",
        _ => status.ToString()
    };
}
