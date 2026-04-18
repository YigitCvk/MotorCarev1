using MediatR;

namespace MotorCare.Application.Tenants.Commands.CreateTenant;

public sealed record CreateTenantCommand(string Identifier, string Name) : IRequest<Guid>;
