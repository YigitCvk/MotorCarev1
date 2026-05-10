using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.SendDisableTwoFactorEmailCode;

public sealed class SendDisableTwoFactorEmailCodeCommandHandler : IRequestHandler<SendDisableTwoFactorEmailCodeCommand, AuthActionMessageDto>
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ISecurityTokenFactory _securityTokenFactory;
    private readonly ILogger<SendDisableTwoFactorEmailCodeCommandHandler> _logger;

    public SendDisableTwoFactorEmailCodeCommandHandler(
        ICurrentUserProvider currentUserProvider,
        IUserRepository userRepository,
        IEmailSender emailSender,
        ISecurityTokenFactory securityTokenFactory,
        ILogger<SendDisableTwoFactorEmailCodeCommandHandler> logger)
    {
        _currentUserProvider = currentUserProvider;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _securityTokenFactory = securityTokenFactory;
        _logger = logger;
    }

    public async Task<AuthActionMessageDto> Handle(SendDisableTwoFactorEmailCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        if (!user.TwoFactorEnabled)
        {
            return new AuthActionMessageDto("İki aşamalı doğrulama zaten devre dışı.");
        }

        var now = DateTimeOffset.UtcNow;
        var latestOtp = await _userRepository.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.TwoFactorDisableEmailOtp, cancellationToken);
        if (latestOtp is not null && latestOtp.CreatedAt >= now.AddMinutes(-1))
        {
            return new AuthActionMessageDto("Lutfen yeni kod istemeden önce kisa sure bekleyin.");
        }

        user.RevokeSecurityTokens(UserSecurityTokenPurpose.TwoFactorDisableEmailOtp, now);
        var code = _securityTokenFactory.GenerateNumericCode();
        var otp = user.AddSecurityToken(
            UserSecurityTokenPurpose.TwoFactorDisableEmailOtp,
            _securityTokenFactory.Hash(code),
            now.AddMinutes(10),
            now);

        _userRepository.Update(user);
        _userRepository.AddSecurityToken(otp);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Auth.TwoFactorEmailSendRequested,
            "Two-factor disable email requested. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
            user.Id,
            otp.ExpiresAt);

        try
        {
            await _emailSender.SendTwoFactorCodeAsync(user.Email, user.FullName, code, otp.ExpiresAt.UtcDateTime, cancellationToken);
            _logger.LogInformation(
                EventIdStore.Auth.TwoFactorEmailSent,
                "Two-factor disable email sent. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                otp.ExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                EventIdStore.Auth.TwoFactorEmailSendFailed,
                ex,
                "Two-factor disable email send failed. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                otp.ExpiresAt);
        }

        return new AuthActionMessageDto("Doğrulama kodu e-posta adresinize gönderildi.");
    }

    private async Task<Domain.Users.User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.GetUserId()
            ?? throw new UnauthorizedAccessException("Current user is not available.");

        var tenantIdentifier = _currentUserProvider.GetTenantIdentifier()
            ?? throw new UnauthorizedAccessException("Current tenant is not available.");

        var user = await _userRepository.GetByIdAsync(userId, tenantIdentifier, cancellationToken)
            ?? throw new UnauthorizedAccessException("Current user is not available.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("The user is inactive.");
        }

        return user;
    }
}
