using MediatR;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleServiceHistory;

public sealed record GetVehicleServiceHistoryQuery(Guid VehicleId) : IRequest<VehicleServiceHistoryDto?>;

public sealed record VehicleServiceHistoryDto(
    Guid VehicleId,
    string Plate,
    string Brand,
    string Model,
    int Year,
    string VehicleDisplay,
    int? CurrentKm,
    int TotalServiceOrderCount,
    DateTimeOffset? LastServiceDate,
    decimal TotalSpent,
    IReadOnlyList<VehicleServiceHistoryItemDto> ServiceOrders);

public sealed record VehicleServiceHistoryItemDto(
    Guid Id,
    string OrderNo,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    string Status,
    string StatusText,
    string? Complaint,
    decimal GrandTotal,
    decimal PaidTotal,
    decimal RemainingTotal);
