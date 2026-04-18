using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.ServiceOrders.Commands.AddPaymentToOrder;

public sealed record AddPaymentToOrderCommand(
    Guid Id,
    decimal Amount,
    PaymentMethod Method,
    DateTimeOffset? PaymentDate) : IRequest<Unit>;
