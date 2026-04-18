using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.RemovePartFromOrder;

public sealed record RemovePartFromOrderCommand(Guid Id, Guid PartId) : IRequest<Unit>;
