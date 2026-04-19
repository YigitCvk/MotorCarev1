using MediatR;
using MotorCare.Application.Common.Models;
using MotorCare.Application.Customers.Queries.GetCustomerById;

namespace MotorCare.Application.Customers.Queries.SearchCustomers;

public sealed record SearchCustomersQuery(
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<CustomerDto>>;
