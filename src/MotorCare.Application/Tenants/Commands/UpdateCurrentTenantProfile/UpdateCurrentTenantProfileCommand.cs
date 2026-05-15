using MediatR;
using MotorCare.Application.Tenants.Queries.GetCurrentTenantProfile;

namespace MotorCare.Application.Tenants.Commands.UpdateCurrentTenantProfile;

public sealed record UpdateCurrentTenantProfileCommand(
    string Name,
    string? LegalName,
    string? LogoUrl,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxOffice,
    string? TaxNumber,
    string? Website) : IRequest<TenantProfileDto>;
