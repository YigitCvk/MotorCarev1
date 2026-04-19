using MediatR;
using MotorCare.Application.Appointments;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Dashboard.Queries.GetDailySummary;

public class GetDailySummaryQueryHandler : IRequestHandler<GetDailySummaryQuery, DailySummaryDto>
{
    private readonly IServiceOrderRepository _serviceOrders;
    private readonly IAppointmentRepository _appointments;
    private readonly ICustomerRepository _customers;
    private readonly IVehicleRepository _vehicles;
    private readonly ITenantProvider _tenantProvider;

    public GetDailySummaryQueryHandler(
        IServiceOrderRepository serviceOrders,
        IAppointmentRepository appointments,
        ICustomerRepository customers,
        IVehicleRepository vehicles,
        ITenantProvider tenantProvider)
    {
        _serviceOrders = serviceOrders;
        _appointments = appointments;
        _customers = customers;
        _vehicles = vehicles;
        _tenantProvider = tenantProvider;
    }

    public async Task<DailySummaryDto> Handle(GetDailySummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var (todayStart, todayEnd) = GetIstanbulDayRange();
        var summary = await _serviceOrders.GetDailySummaryAsync(tenantId, cancellationToken);
        var todayAppointments = await _appointments.GetFilteredAsync(
            tenantId,
            todayStart,
            todayEnd,
            null,
            null,
            null,
            cancellationToken);
        var (recentOrders, _) = await _serviceOrders.GetFilteredPagedAsync(
            tenantId,
            null,
            null,
            null,
            null,
            null,
            1,
            5,
            cancellationToken);

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

        foreach (var order in recentOrders.OrderByDescending(x => x.OpenedAt))
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

        return new DailySummaryDto(
            appointmentItems.Count,
            summary.ActiveServiceOrders,
            summary.CompletedServiceOrdersToday,
            summary.DeliveryWaitingCount,
            summary.TotalPaymentsToday,
            summary.TotalPaymentsToday,
            summary.PendingAmount,
            appointmentItems,
            recentOrderItems);
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
        ServiceOrderStatus.Completed => "Tamamlandı",
        ServiceOrderStatus.Delivered => "Teslim Edildi",
        ServiceOrderStatus.Cancelled => "İptal",
        _ => status.ToString()
    };
}
