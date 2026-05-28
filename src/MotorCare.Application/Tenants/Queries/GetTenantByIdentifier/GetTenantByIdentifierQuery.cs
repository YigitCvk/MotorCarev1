using MediatR;

namespace MotorCare.Application.Tenants.Queries.GetTenantByIdentifier;

public sealed record TenantDto(
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
    bool IsActive);

public sealed record GetTenantByIdentifierQuery(string Identifier) : IRequest<TenantDto?>;
