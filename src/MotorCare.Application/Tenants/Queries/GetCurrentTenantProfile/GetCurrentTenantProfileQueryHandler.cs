using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Tenants;

namespace MotorCare.Application.Tenants.Queries.GetCurrentTenantProfile;

public sealed class GetCurrentTenantProfileQueryHandler
    : IRequestHandler<GetCurrentTenantProfileQuery, TenantProfileDto?>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetCurrentTenantProfileQueryHandler(
        ITenantRepository tenantRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _tenantRepository = tenantRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<TenantProfileDto?> Handle(
        GetCurrentTenantProfileQuery request,
        CancellationToken cancellationToken)
    {
        var tenantIdentifier = _currentUserProvider.GetTenantIdentifier()
            ?? throw new UnauthorizedAccessException("Tenant identifier is required.");

        var tenant = await _tenantRepository.GetByIdentifierAsync(tenantIdentifier, cancellationToken);
        return tenant is null ? null : ToDto(tenant);
    }

    internal static TenantProfileDto ToDto(Tenant tenant)
        => new(
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
            tenant.Website);
}
