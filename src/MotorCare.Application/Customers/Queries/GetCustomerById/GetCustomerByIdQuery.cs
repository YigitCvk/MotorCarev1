using MediatR;

namespace MotorCare.Application.Customers.Queries.GetCustomerById;

public sealed record CustomerDto(
    Guid Id,
    string FullName,
    string? Phone,
    string? Email,
    string? Whatsapp,
    string? Notes);

public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;
