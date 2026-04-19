using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Customers.Queries.GetCustomerSummary;

public sealed record GetCustomerSummaryQuery(Guid CustomerId) : IRequest<CustomerSummaryDto?>;

public sealed record CustomerSummaryDto(
    Guid Id,
    string FullName,
    string? Phone,
    string? Email,
    string? Whatsapp,
    string? Notes,
    IReadOnlyList<CustomerVehicleSummaryDto> Vehicles,
    IReadOnlyList<CustomerAppointmentSummaryDto> Appointments,
    IReadOnlyList<CustomerServiceOrderSummaryDto> ServiceOrders);

public sealed record CustomerVehicleSummaryDto(
    Guid Id,
    string Plate,
    string Brand,
    string Model,
    int Year,
    string VehicleDisplay,
    int? CurrentKm,
    DateTimeOffset? LastServiceDate);

public sealed record CustomerAppointmentSummaryDto(
    Guid Id,
    AppointmentType Type,
    string TypeText,
    AppointmentStatus Status,
    string StatusText,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Plate,
    Guid? ServiceOrderId);

public sealed record CustomerServiceOrderSummaryDto(
    Guid Id,
    string OrderNo,
    string Status,
    string StatusText,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    string? VehiclePlate,
    string? Complaint,
    decimal GrandTotal,
    decimal PaidTotal,
    IReadOnlyList<CustomerOperationSummaryDto> Operations,
    IReadOnlyList<CustomerPartSummaryDto> Parts,
    IReadOnlyList<CustomerPaymentSummaryDto> Payments);

public sealed record CustomerOperationSummaryDto(string Description, decimal Price);

public sealed record CustomerPartSummaryDto(
    string PartName,
    string? PartNumber,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public sealed record CustomerPaymentSummaryDto(
    decimal Amount,
    PaymentMethod Method,
    string MethodText,
    DateTimeOffset PaymentDate);
