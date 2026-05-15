using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Users.Commands.InviteUser;

public sealed class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ISecurityTokenFactory _securityTokenFactory;
    private readonly IAuthLinkBuilder _authLinkBuilder;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<InviteUserCommandHandler> _logger;

    public InviteUserCommandHandler(
        IUserRepository userRepository,
        ITenantProvider tenantProvider,
        ISecurityTokenFactory securityTokenFactory,
        IAuthLinkBuilder authLinkBuilder,
        IEmailSender emailSender,
        ILogger<InviteUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tenantProvider = tenantProvider;
        _securityTokenFactory = securityTokenFactory;
        _authLinkBuilder = authLinkBuilder;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Unit> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;

        var existing = await _userRepository.GetByEmailWithSecurityTokensAsync(tenantId, normalizedEmail, cancellationToken);

        User targetUser;
        if (existing is not null)
        {
            // Active/verified user — real conflict, cannot re-invite
            if (existing.IsEmailVerified)
                throw new ConflictException($"Bu e-posta adresiyle zaten aktif bir kullanıcı mevcut: '{normalizedEmail}'");

            // Pending invite user — revoke old tokens and resend
            existing.UpdatePendingInvite(request.FullName, request.Role);
            existing.RevokeSecurityTokens(UserSecurityTokenPurpose.UserInvitation, now);
            _userRepository.Update(existing);
            await _userRepository.SaveChangesAsync(cancellationToken);
            targetUser = existing;
        }
        else
        {
            var pendingFullName = string.IsNullOrWhiteSpace(request.FullName)
                ? "Davet Bekliyor"
                : request.FullName.Trim();
            var pendingUser = new User(tenantId, pendingFullName, normalizedEmail, $"INVITE_{Guid.NewGuid():N}", request.Role);
            await _userRepository.AddAsync(pendingUser, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
            targetUser = pendingUser;
        }

        var rawToken = _securityTokenFactory.GenerateOpaqueToken();
        var expiresAt = now.AddHours(48);
        var securityToken = targetUser.AddSecurityToken(
            UserSecurityTokenPurpose.UserInvitation,
            _securityTokenFactory.Hash(rawToken),
            expiresAt,
            now);

        _userRepository.Update(targetUser);
        _userRepository.AddSecurityToken(securityToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.User.UserInviteSent,
            "User invitation sent. UserId={UserId} TenantId={TenantId} Role={Role}",
            targetUser.Id,
            tenantId,
            request.Role);

        var inviteUrl = _authLinkBuilder.BuildInviteUrl(rawToken);

        try
        {
            await _emailSender.SendUserInvitationAsync(normalizedEmail, tenantId, inviteUrl, expiresAt.UtcDateTime, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User invitation email send failed. UserId={UserId}", targetUser.Id);
        }

        return Unit.Value;
    }
}
