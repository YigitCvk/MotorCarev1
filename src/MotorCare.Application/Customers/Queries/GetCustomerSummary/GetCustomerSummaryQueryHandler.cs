using MediatR;
using MotorCare.Application.Appointments;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Customers.Queries.GetCustomerSummary;

public sealed class GetCustomerSummaryQueryHandler : IRequestHandler<GetCustomerSummaryQuery, CustomerSummaryDto?>
{
    private readonly ICustomerRepository _customers;
    private readonly IVehicleRepository _vehicles;
    private readonly IAppointmentRepository _appointments;
    private readonly IServiceOrderRepository _serviceOrders;
    private readonly ITenantProvider _tenantProvider;

    public GetCustomerSummaryQueryHandler(
        ICustomerRepository customers,
        IVehicleRepository vehicles,
        IAppointmentRepository appointments,
        IServiceOrderRepository serviceOrders,
        ITenantProvider tenantProvider)
    {
        _customers = customers;
        _vehicles = vehicles;
        _appointments = appointments;
        _serviceOrders = serviceOrders;
        _tenantProvider = tenantProvider;
    }

    public async Task<CustomerSummaryDto?> Handle(GetCustomerSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var customer = await _customers.GetByIdAsync(request.CustomerId, tenantId, cancellationToken);
        if (customer is null) return null;

        // Sequential awaits are required: all repositories share one scoped DbContext
        // which is not thread-safe — parallel Task.WhenAll would cause a concurrency error.
        var vehicles = await _vehicles.GetByCustomerIdAsync(tenantId, request.CustomerId, cancellationToken);
        var appointments = await _appointments.GetByCustomerIdAsync(tenantId, request.CustomerId, cancellationToken);
        var orders = await _serviceOrders.GetFilteredAsync(tenantId, request.CustomerId, null, null, null, null, cancellationToken);

        // Build a map of vehicleId → plate for service order enrichment
        var vehiclePlateMap = vehicles.ToDictionary(v => v.Id, v => v.Plate.OriginalValue);

        // For service orders that reference vehicles not in the customer's current vehicle list
        // (e.g. vehicle was reassigned), we still need the plate. We look it up from order history
        // via the map; if absent, we leave it null.

        var vehicleDtos = vehicles.Select(v =>
        {
            var lastService = orders
                .Where(o => o.VehicleId == v.Id)
                .OrderByDescending(o => o.OpenedAt)
                .FirstOrDefault();

            return new CustomerVehicleSummaryDto(
                v.Id,
                v.Plate.OriginalValue,
                v.Brand,
                v.Model,
                v.Year,
                $"{v.Plate.OriginalValue} · {v.Brand} {v.Model} ({v.Year})",
                v.CurrentKm,
                lastService?.OpenedAt);
        }).ToList();

        var appointmentDtos = appointments.Select(a => new CustomerAppointmentSummaryDto(
            a.Id,
            a.Type,
            AppointmentTextMapper.ToText(a.Type),
            a.Status,
            AppointmentTextMapper.ToText(a.Status),
            a.StartAt,
            a.EndAt,
            a.Plate,
            a.ServiceOrderId)).ToList();

        var orderDtos = orders.Select(o =>
        {
            vehiclePlateMap.TryGetValue(o.VehicleId, out var plate);

            var operationDtos = o.Operations
                .Select(op => new CustomerOperationSummaryDto(op.Description, op.Price))
                .ToList();

            var partDtos = o.Parts
                .Select(p => new CustomerPartSummaryDto(p.PartName, p.PartNumber, p.UnitPrice, p.Quantity, p.TotalPrice))
                .ToList();

            var paymentDtos = o.Payments
                .Select(p => new CustomerPaymentSummaryDto(p.Amount, p.Method, ToPaymentMethodText(p.Method), p.PaymentDate))
                .ToList();

            return new CustomerServiceOrderSummaryDto(
                o.Id,
                o.OrderNo,
                o.Status.ToString(),
                ToStatusText(o.Status),
                o.OpenedAt,
                o.ClosedAt,
                plate,
                o.Complaint,
                o.GrandTotal,
                o.PaidTotal,
                operationDtos,
                partDtos,
                paymentDtos);
        }).ToList();

        return new CustomerSummaryDto(
            customer.Id,
            customer.FullName,
            customer.Phone?.Value,
            customer.Email,
            customer.Whatsapp?.Value,
            customer.Notes,
            vehicleDtos,
            appointmentDtos,
            orderDtos);
    }

    private static string ToStatusText(ServiceOrderStatus status) => status switch
    {
        ServiceOrderStatus.Open => "Açık",
        ServiceOrderStatus.InProgress => "İşlemde",
        ServiceOrderStatus.WaitingForParts => "Parça Bekliyor",
        ServiceOrderStatus.Completed => "Tamamlandı",
        ServiceOrderStatus.Delivered => "Teslim Edildi",
        ServiceOrderStatus.Cancelled => "İptal",
        _ => status.ToString()
    };

    private static string ToPaymentMethodText(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "Nakit",
        PaymentMethod.CreditCard => "Kredi Kartı",
        PaymentMethod.BankTransfer => "Banka Transferi",
        _ => method.ToString()
    };
}
