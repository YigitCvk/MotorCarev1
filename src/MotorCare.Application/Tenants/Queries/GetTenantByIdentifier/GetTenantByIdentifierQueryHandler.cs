using MediatR;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Tenants.Queries.GetTenantByIdentifier;

public class GetTenantByIdentifierQueryHandler : IRequestHandler<GetTenantByIdentifierQuery, TenantDto?>
{
    private readonly ITenantRepository _repository;

    public GetTenantByIdentifierQueryHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdentifierQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdentifierAsync(request.Identifier, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        return new TenantDto(
            tenant.Id,
            tenant.Identifier,
            tenant.Name,
            tenant.LegalName,
            tenant.LogoUrl,
            tenant.Phone,
            tenant.Email,
            tenant.Address,
            tenant.TaxOffice,
            tenant.TaxNumber,
            tenant.IsActive);
    }
}
