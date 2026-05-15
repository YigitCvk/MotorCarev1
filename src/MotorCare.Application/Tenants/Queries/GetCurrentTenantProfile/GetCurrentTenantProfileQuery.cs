using MediatR;

namespace MotorCare.Application.Tenants.Queries.GetCurrentTenantProfile;

public sealed record TenantProfileDto(
    Guid Id,
    string Identifier,
    string Name,
    string? LegalName,
    string? LogoUrl,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxOffice,
    string? TaxNumber,
    string? Website);

public sealed record GetCurrentTenantProfileQuery : IRequest<TenantProfileDto?>;
