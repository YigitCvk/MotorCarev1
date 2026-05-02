using MediatR;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderStatusHistory;

public sealed record GetServiceOrderStatusHistoryQuery(Guid ServiceOrderId)
    : IRequest<IReadOnlyList<ServiceOrderStatusHistoryDto>>;

public sealed record ServiceOrderStatusHistoryDto(
    Guid Id,
    string? FromStatus,
    string? FromStatusText,
    string ToStatus,
    string ToStatusText,
    string? Note,
    string? ChangedByUserName,
    DateTimeOffset CreatedAt);
