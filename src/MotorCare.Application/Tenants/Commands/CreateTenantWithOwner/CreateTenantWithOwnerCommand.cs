using MediatR;

namespace MotorCare.Application.Tenants.Commands.CreateTenantWithOwner;

public sealed record CreateTenantWithOwnerResultDto(
    Guid TenantId,
    string TenantIdentifier,
    Guid OwnerId,
    string OwnerEmail,
    bool VerificationEmailSent);

public sealed record CreateTenantWithOwnerCommand(
    string TenantIdentifier,
    string TenantName,
    string OwnerFullName,
    string OwnerEmail,
    string OwnerPassword) : IRequest<CreateTenantWithOwnerResultDto>;
