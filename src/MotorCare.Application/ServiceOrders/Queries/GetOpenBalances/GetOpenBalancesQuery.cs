using MediatR;

namespace MotorCare.Application.ServiceOrders.Queries.GetOpenBalances;

public sealed record GetOpenBalancesQuery(int Take = 50) : IRequest<IReadOnlyList<OpenBalanceDto>>;

public sealed record OpenBalanceDto(
    Guid ServiceOrderId,
    string OrderNo,
    string? CustomerName,
    string? VehiclePlate,
    decimal GrandTotal,
    decimal PaidTotal,
    decimal RemainingTotal,
    string Status,
    DateTimeOffset OpenedAt);
