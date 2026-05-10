using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Appointments;
using MotorCare.Application.Common;
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
    private readonly ILogger<GetCustomerSummaryQueryHandler> _logger;

    public GetCustomerSummaryQueryHandler(
        ICustomerRepository customers,
        IVehicleRepository vehicles,
        IAppointmentRepository appointments,
        IServiceOrderRepository serviceOrders,
        ITenantProvider tenantProvider,
        ILogger<GetCustomerSummaryQueryHandler> logger)
    {
        _customers = customers;
        _vehicles = vehicles;
        _appointments = appointments;
        _serviceOrders = serviceOrders;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<CustomerSummaryDto?> Handle(GetCustomerSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var customer = await _customers.GetByIdAsync(request.CustomerId, tenantId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        var vehicles = await _vehicles.GetByCustomerIdAsync(tenantId, request.CustomerId, cancellationToken);
        var appointments = await _appointments.GetByCustomerIdAsync(tenantId, request.CustomerId, cancellationToken);
        var orders = await _serviceOrders.GetFilteredAsync(tenantId, request.CustomerId, null, null, null, null, cancellationToken);

        var vehiclePlateMap = vehicles.ToDictionary(v => v.Id, v => v.Plate.OriginalValue);

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

        _logger.LogInformation(
            EventIdStore.Customer.CustomerSummaryFetched,
            "Customer summary fetched for customer {CustomerId} in tenant {TenantId}. VehicleCount={VehicleCount} AppointmentCount={AppointmentCount} ServiceOrderCount={ServiceOrderCount}",
            customer.Id,
            tenantId,
            vehicleDtos.Count,
            appointmentDtos.Count,
            orderDtos.Count);

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
        ServiceOrderStatus.Open => "Acik",
        ServiceOrderStatus.InProgress => "Islemde",
        ServiceOrderStatus.WaitingForParts => "Parça Bekliyor",
        ServiceOrderStatus.Completed => "Teslime Hazır",
        ServiceOrderStatus.Delivered => "Teslim Edildi",
        ServiceOrderStatus.Cancelled => "Iptal",
        _ => status.ToString()
    };

    private static string ToPaymentMethodText(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "Nakit",
        PaymentMethod.CreditCard => "Kredi Karti",
        PaymentMethod.BankTransfer => "Banka Transferi",
        _ => method.ToString()
    };
}
