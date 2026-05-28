using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Tenants.Queries.GetCurrentTenantProfile;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Tenants.Commands.UpdateCurrentTenantProfile;

public sealed class UpdateCurrentTenantProfileCommandHandler
    : IRequestHandler<UpdateCurrentTenantProfileCommand, TenantProfileDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public UpdateCurrentTenantProfileCommandHandler(
        ITenantRepository tenantRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _tenantRepository = tenantRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<TenantProfileDto> Handle(
        UpdateCurrentTenantProfileCommand request,
        CancellationToken cancellationToken)
    {
        var tenantIdentifier = _currentUserProvider.GetTenantIdentifier()
            ?? throw new UnauthorizedAccessException("Tenant identifier is required.");

        var tenant = await _tenantRepository.GetByIdentifierAsync(tenantIdentifier, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Tenants.Tenant), tenantIdentifier);

        tenant.UpdateProfile(
            request.Name,
            request.LegalName,
            request.LogoUrl,
            request.Phone,
            request.Email,
            request.Address,
            request.TaxOffice,
            request.TaxNumber,
            request.Website);

        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        return GetCurrentTenantProfileQueryHandler.ToDto(tenant);
    }
}
