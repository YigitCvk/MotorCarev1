using MediatR;

namespace MotorCare.Application.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string FullName,
    string Phone,
    string? Email,
    string? Whatsapp,
    string? Notes) : IRequest<Guid>;
