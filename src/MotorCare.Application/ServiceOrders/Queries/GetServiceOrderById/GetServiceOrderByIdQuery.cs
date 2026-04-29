using MediatR;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderById;

public sealed record ServiceOperationItemDto(Guid Id, string Description, decimal Price);

public sealed record ServicePartItemDto(Guid Id, string PartName, string? PartNumber, decimal UnitPrice, int Quantity, decimal TotalPrice);

public sealed record ServiceConsumableItemDto(
    Guid Id,
    string Category,
    string Brand,
    string ProductName,
    string? SubCategory,
    string? Specification,
    string? Notes);

public sealed record ServicePaymentDto(Guid Id, decimal Amount, string Method, DateTimeOffset PaymentDate);

public sealed record ServiceOrderDto(
    Guid Id,
    string OrderNo,
    Guid VehicleId,
    Guid CustomerId,
    string? CustomerName,
    string? VehiclePlate,
    string? VehicleDisplay,
    string Status,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    int VehicleKm,
    string? Complaint,
    string? WorkDescription,
    string? InternalNote,
    decimal LaborTotal,
    decimal PartsTotal,
    decimal DiscountTotal,
    decimal GrandTotal,
    decimal PaidTotal,
    decimal RemainingTotal,
    IReadOnlyList<ServiceOperationItemDto> Operations,
    IReadOnlyList<ServicePartItemDto> Parts,
    IReadOnlyList<ServiceConsumableItemDto> Consumables,
    IReadOnlyList<ServicePaymentDto> Payments);

public sealed record GetServiceOrderByIdQuery(Guid Id) : IRequest<ServiceOrderDto?>;
