using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.ServiceOrders.Commands.UpdateServiceOrderStatus;

public sealed record UpdateServiceOrderStatusCommand(Guid Id, ServiceOrderStatus Status, string? Note = null) : IRequest<Unit>;
