using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.SetOrderDiscount;

public sealed record SetOrderDiscountCommand(Guid Id, decimal Discount) : IRequest<Unit>;
