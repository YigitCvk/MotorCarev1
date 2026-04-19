using MediatR;
using MotorCare.Application.Common.Models;
using MotorCare.Domain.Enums;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderById;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrders;

public sealed record GetServiceOrdersQuery(
    Guid? CustomerId,
    ServiceOrderStatus? Status,
    string? SearchText,
    DateTimeOffset? OpenedFrom,
    DateTimeOffset? OpenedTo,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<ServiceOrderDto>>;
