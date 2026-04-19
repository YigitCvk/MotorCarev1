using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;
using MotorCare.Application.Customers.Queries.GetCustomerById;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Customers.Queries.SearchCustomers;

public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public SearchCustomersQueryHandler(ICustomerRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<PagedResult<CustomerDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var pagination = PaginationRequest.Of(request.PageNumber, request.PageSize);

        var (customers, totalCount) = await _repository.SearchPagedAsync(
            tenantId,
            request.SearchTerm,
            pagination.SafePageNumber,
            pagination.SafePageSize,
            cancellationToken);

        var items = customers
            .Select(c => new CustomerDto(c.Id, c.FullName, c.Phone?.Value, c.Email, c.Whatsapp?.Value, c.Notes))
            .ToList();

        return PagedResult<CustomerDto>.Create(items, pagination.SafePageNumber, pagination.SafePageSize, totalCount);
    }
}
