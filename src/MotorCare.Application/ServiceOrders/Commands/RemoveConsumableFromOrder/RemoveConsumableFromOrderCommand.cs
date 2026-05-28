using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.RemoveConsumableFromOrder;

public sealed record RemoveConsumableFromOrderCommand(Guid Id, Guid ConsumableId) : IRequest<Unit>;
