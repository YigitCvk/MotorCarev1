using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users;

namespace MotorCare.Application.Users.Commands.CreateTenantUser;

public sealed class CreateTenantUserCommandHandler : IRequestHandler<CreateTenantUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateTenantUserCommandHandler> _logger;

    public CreateTenantUserCommandHandler(
        IUserRepository userRepository,
        ITenantProvider tenantProvider,
        IPasswordHasher passwordHasher,
        ILogger<CreateTenantUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tenantProvider = tenantProvider;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateTenantUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existing = await _userRepository.GetByEmailAsync(tenantId, normalizedEmail, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"Bu e-posta adresiyle zaten bir kullanıcı mevcut: '{normalizedEmail}'");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User(tenantId, request.FullName, normalizedEmail, passwordHash, request.Role);
        user.MarkEmailVerified();

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.User.UserCreated,
            "Tenant user created. UserId={UserId} TenantId={TenantId} Role={Role}",
            user.Id,
            tenantId,
            request.Role);

        return user.Id;
    }
}
