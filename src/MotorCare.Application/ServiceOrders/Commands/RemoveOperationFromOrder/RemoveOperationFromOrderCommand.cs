using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.RemoveOperationFromOrder;

public sealed record RemoveOperationFromOrderCommand(Guid Id, Guid OperationId) : IRequest<Unit>;
