using MediatR;

namespace MotorCare.Application.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string FullName,
    string Phone,
    string? Email,
    string? Whatsapp,
    string? Notes) : IRequest<Unit>;
