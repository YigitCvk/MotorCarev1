using MediatR;
using MotorCare.Application.Customers.Queries.GetCustomerById;

namespace MotorCare.Application.Customers.Queries.SearchCustomers;

public sealed record SearchCustomersQuery(string? SearchTerm) : IRequest<IReadOnlyList<CustomerDto>>;
