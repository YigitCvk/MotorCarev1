using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleServiceHistory;

public sealed class GetVehicleServiceHistoryQueryHandler : IRequestHandler<GetVehicleServiceHistoryQuery, VehicleServiceHistoryDto?>
{
    private readonly IVehicleRepository _vehicles;
    private readonly IServiceOrderRepository _serviceOrders;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<GetVehicleServiceHistoryQueryHandler> _logger;

    public GetVehicleServiceHistoryQueryHandler(
        IVehicleRepository vehicles,
        IServiceOrderRepository serviceOrders,
        ITenantProvider tenantProvider,
        ILogger<GetVehicleServiceHistoryQueryHandler> logger)
    {
        _vehicles = vehicles;
        _serviceOrders = serviceOrders;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<VehicleServiceHistoryDto?> Handle(GetVehicleServiceHistoryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var vehicle = await _vehicles.GetByIdAsync(request.VehicleId, tenantId, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        var (orders, totalCount) = await _serviceOrders.GetByVehicleIdAsync(request.VehicleId, tenantId, 1, 50, cancellationToken);
        var orderItems = orders
            .OrderByDescending(x => x.OpenedAt)
            .Select(order => new VehicleServiceHistoryItemDto(
                order.Id,
                order.OrderNo,
                order.OpenedAt,
                order.ClosedAt,
                order.Status.ToString(),
                ToServiceOrderStatusText(order.Status),
                order.Complaint,
                order.GrandTotal,
                order.PaidTotal,
                order.RemainingTotal))
            .ToList();

        _logger.LogInformation(
            EventIdStore.Vehicle.VehicleServiceHistoryFetched,
            "Vehicle service history fetched. VehicleId={VehicleId} TenantId={TenantId} ServiceOrderCount={ServiceOrderCount}",
            request.VehicleId,
            tenantId,
            totalCount);

        return new VehicleServiceHistoryDto(
            vehicle.Id,
            vehicle.Plate.OriginalValue,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            $"{vehicle.Plate.OriginalValue} · {vehicle.Brand} {vehicle.Model} ({vehicle.Year})",
            vehicle.CurrentKm,
            totalCount,
            orderItems.FirstOrDefault()?.OpenedAt,
            orderItems.Sum(x => x.GrandTotal),
            orderItems);
    }

    private static string ToServiceOrderStatusText(ServiceOrderStatus status) => status switch
    {
        ServiceOrderStatus.Open => "Açık",
        ServiceOrderStatus.InProgress => "İşlemde",
        ServiceOrderStatus.WaitingForParts => "Parça Bekliyor",
        ServiceOrderStatus.Completed => "Teslime Hazır",
        ServiceOrderStatus.Delivered => "Teslim Edildi",
        ServiceOrderStatus.Cancelled => "İptal",
        _ => status.ToString()
    };
}
