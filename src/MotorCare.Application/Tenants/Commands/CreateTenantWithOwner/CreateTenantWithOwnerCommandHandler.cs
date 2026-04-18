using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;

namespace MotorCare.Application.Tenants.Commands.CreateTenantWithOwner;

public class CreateTenantWithOwnerCommandHandler : IRequestHandler<CreateTenantWithOwnerCommand, CreateTenantWithOwnerResultDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateTenantWithOwnerCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateTenantWithOwnerResultDto> Handle(CreateTenantWithOwnerCommand request, CancellationToken cancellationToken)
    {
        var existingTenant = await _tenantRepository.GetByIdentifierAsync(request.TenantIdentifier, cancellationToken);
        if (existingTenant is not null)
        {
            throw new ConflictException("A tenant with this identifier already exists.");
        }

        var normalizedEmail = request.OwnerEmail.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(request.TenantIdentifier, normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException("A user with this email already exists for the tenant.");
        }

        var tenant = new Tenant(request.TenantIdentifier, request.TenantName);
        var passwordHash = _passwordHasher.Hash(request.OwnerPassword);
        var owner = new User(tenant.Identifier, request.OwnerFullName, request.OwnerEmail, passwordHash, UserRole.Owner);

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _userRepository.AddAsync(owner, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new CreateTenantWithOwnerResultDto(tenant.Id, tenant.Identifier, owner.Id, owner.Email);
    }
}
