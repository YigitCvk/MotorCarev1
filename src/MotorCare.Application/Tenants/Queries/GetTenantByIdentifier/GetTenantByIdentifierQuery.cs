using MediatR;

namespace MotorCare.Application.Tenants.Queries.GetTenantByIdentifier;

public sealed record TenantDto(Guid Id, string Identifier, string Name, bool IsActive);

public sealed record GetTenantByIdentifierQuery(string Identifier) : IRequest<TenantDto?>;
