using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.AddOperationToOrder;

public sealed record AddOperationToOrderCommand(Guid Id, string Description, decimal Price) : IRequest<Unit>;
