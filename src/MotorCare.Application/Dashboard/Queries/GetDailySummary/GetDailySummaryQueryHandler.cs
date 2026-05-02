using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Appointments;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.Dashboard.Queries.GetDailySummary;

public class GetDailySummaryQueryHandler : IRequestHandler<GetDailySummaryQuery, DailySummaryDto>
{
    private readonly IServiceOrderRepository _serviceOrders;
    private readonly IAppointmentRepository _appointments;
    private readonly ICustomerRepository _customers;
    private readonly IVehicleRepository _vehicles;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<GetDailySummaryQueryHandler> _logger;

    public GetDailySummaryQueryHandler(
        IServiceOrderRepository serviceOrders,
        IAppointmentRepository appointments,
        ICustomerRepository customers,
        IVehicleRepository vehicles,
        ITenantProvider tenantProvider,
        ILogger<GetDailySummaryQueryHandler> logger)
    {
        _serviceOrders = serviceOrders;
        _appointments = appointments;
        _customers = customers;
        _vehicles = vehicles;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<DailySummaryDto> Handle(GetDailySummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var (todayStart, todayEnd) = GetIstanbulDayRange();
        var partialFallbackApplied = false;
        var summary = await TryGetDailySummaryAsync(tenantId, cancellationToken);
        partialFallbackApplied |= summary.IsFallback;
        var todayAppointments = await _appointments.GetFilteredAsync(
            tenantId,
            todayStart,
            todayEnd,
            null,
            null,
            null,
            cancellationToken);
        var recentOrders = await TryGetRecentDashboardOrdersAsync(tenantId, cancellationToken);
        partialFallbackApplied |= recentOrders.IsFallback;

        var appointmentItems = todayAppointments
            .OrderBy(x => x.StartAt)
            .Select(a => new DashboardAppointmentItemDto(
                a.Id,
                a.CustomerName,
                a.Phone,
                a.Plate,
                (int)a.Type,
                ToAppointmentTypeText(a.Type),
                (int)a.Status,
                ToAppointmentStatusText(a.Status),
                a.StartAt,
                a.EndAt))
            .ToList();

        var customerCache = new Dictionary<Guid, string?>();
        var vehicleCache = new Dictionary<Guid, string?>();
        var recentOrderItems = new List<DashboardServiceOrderItemDto>();

        foreach (var order in recentOrders.Items.OrderByDescending(x => x.OpenedAt))
        {
            string? customerName = null;
            if (!customerCache.TryGetValue(order.CustomerId, out customerName))
            {
                customerName = (await _customers.GetByIdAsync(order.CustomerId, tenantId, cancellationToken))?.FullName;
                customerCache[order.CustomerId] = customerName;
            }

            string? vehiclePlate = null;
            if (!vehicleCache.TryGetValue(order.VehicleId, out vehiclePlate))
            {
                vehiclePlate = (await _vehicles.GetByIdAsync(order.VehicleId, tenantId, cancellationToken))?.Plate.OriginalValue;
                vehicleCache[order.VehicleId] = vehiclePlate;
            }

            recentOrderItems.Add(new DashboardServiceOrderItemDto(
                order.Id,
                order.OrderNo,
                customerName,
                vehiclePlate,
                order.Status.ToString(),
                ToServiceOrderStatusText(order.Status),
                order.OpenedAt,
                order.GrandTotal));
        }

        var readyForDeliveryItems = recentOrderItems
            .Where(x => x.Status == ServiceOrderStatus.Completed.ToString())
            .Take(5)
            .ToList();

        var result = new DailySummaryDto(
            appointmentItems.Count,
            summary.Data.ActiveServiceOrders,
            summary.Data.OpenServiceOrders,
            summary.Data.InProgressServiceOrders,
            summary.Data.WaitingForPartsServiceOrders,
            summary.Data.CompletedServiceOrdersToday,
            summary.Data.DeliveryWaitingCount,
            summary.Data.TotalRevenueToday,
            summary.Data.TotalPaymentsToday,
            summary.Data.TotalPaymentsThisMonth,
            summary.Data.PendingAmount,
            appointmentItems,
            recentOrderItems,
            readyForDeliveryItems);

        if (partialFallbackApplied)
        {
            _logger.LogWarning(
                EventIdStore.Dashboard.PartialFallbackApplied,
                "Dashboard partial fallback applied for TenantId={TenantId}. Appointments={AppointmentCount} RecentOrders={RecentOrderCount}",
                tenantId,
                appointmentItems.Count,
                recentOrderItems.Count);
        }

        _logger.LogInformation(
            EventIdStore.Dashboard.DailySummaryFetched,
            "Daily summary fetched. ActiveOrders={ActiveOrders} TodayAppointments={TodayAppointments} TotalPaymentsToday={TotalPaymentsToday}",
            summary.Data.ActiveServiceOrders,
            appointmentItems.Count,
            summary.Data.TotalPaymentsToday);

        return result;
    }

    private async Task<(ServiceOrderDailySummary Data, bool IsFallback)> TryGetDailySummaryAsync(string tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _serviceOrders.GetDailySummaryAsync(tenantId, cancellationToken);
            return (summary, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                EventIdStore.Dashboard.SummaryDataSourceFailed,
                ex,
                "Dashboard summary source failed for TenantId={TenantId}. Falling back to zero summary values.",
                tenantId);

            return (new ServiceOrderDailySummary(0, 0, 0, 0, 0, 0, 0, 0m, 0m, 0m, 0m), true);
        }
    }

    private async Task<(List<ServiceOrderDashboardSnapshot> Items, bool IsFallback)> TryGetRecentDashboardOrdersAsync(string tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _serviceOrders.GetRecentDashboardOrdersAsync(tenantId, 5, cancellationToken);
            return (items, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                EventIdStore.Dashboard.RecentOrdersDataSourceFailed,
                ex,
                "Dashboard recent service orders source failed for TenantId={TenantId}. Falling back to empty recent orders.",
                tenantId);

            return ([], true);
        }
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetIstanbulDayRange()
    {
        var zone = ResolveIstanbulTimeZone();
        var nowInZone = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        var localDate = nowInZone.Date;
        var startLocal = new DateTime(localDate.Year, localDate.Month, localDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var endLocal = startLocal.AddDays(1);

        var startUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(startLocal, zone));
        var endUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(endLocal, zone));
        return (startUtc, endUtc);
    }

    private static TimeZoneInfo ResolveIstanbulTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
    }

    private static string ToAppointmentTypeText(AppointmentType type) => type switch
    {
        AppointmentType.Maintenance => "Bakım",
        AppointmentType.Repair => "Onarım",
        AppointmentType.Cleaning => "Temizlik",
        AppointmentType.Washing => "Yıkama",
        AppointmentType.Inspection => "Kontrol",
        AppointmentType.TireChange => "Lastik Değişimi",
        _ => "Diğer"
    };

    private static string ToAppointmentStatusText(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Scheduled => "Planlandı",
        AppointmentStatus.Confirmed => "Onaylandı",
        AppointmentStatus.CheckedIn => "Servise Geldi",
        AppointmentStatus.ConvertedToOrder => "İş Emrine Dönüştü",
        AppointmentStatus.Cancelled => "İptal",
        AppointmentStatus.NoShow => "Gelmedi",
        AppointmentStatus.Completed => "Tamamlandı",
        _ => status.ToString()
    };

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
