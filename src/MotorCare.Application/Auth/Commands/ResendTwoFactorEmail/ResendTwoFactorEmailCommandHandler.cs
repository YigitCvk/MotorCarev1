using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.ResendTwoFactorEmail;

public sealed class ResendTwoFactorEmailCommandHandler : IRequestHandler<ResendTwoFactorEmailCommand, AuthActionMessageDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ISecurityTokenFactory _securityTokenFactory;
    private readonly ILogger<ResendTwoFactorEmailCommandHandler> _logger;

    public ResendTwoFactorEmailCommandHandler(
        IUserRepository userRepository,
        IEmailSender emailSender,
        ISecurityTokenFactory securityTokenFactory,
        ILogger<ResendTwoFactorEmailCommandHandler> logger)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
        _securityTokenFactory = securityTokenFactory;
        _logger = logger;
    }

    public async Task<AuthActionMessageDto> Handle(ResendTwoFactorEmailCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var ticketHash = _securityTokenFactory.Hash(request.Ticket);
        var challenge = await _userRepository.GetActiveSecurityTokenByHashAsync(ticketHash, UserSecurityTokenPurpose.TwoFactorChallenge, cancellationToken)
            ?? throw new UnauthorizedAccessException("Doğrulama oturumu geçersiz veya süresi dolmuş.");

        var user = await _userRepository.GetByIdAsync(challenge.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Doğrulama oturumu geçersiz veya süresi dolmuş.");

        var latestOtp = await _userRepository.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.TwoFactorEmailOtp, cancellationToken);
        if (latestOtp is not null && latestOtp.CreatedAt >= now.AddMinutes(-1))
        {
            return new AuthActionMessageDto("Lütfen yeni kod istemeden önce kısa süre bekleyin.");
        }

        user.RevokeSecurityTokens(UserSecurityTokenPurpose.TwoFactorEmailOtp, now);
        var code = _securityTokenFactory.GenerateNumericCode();
        var otp = user.AddSecurityToken(
            UserSecurityTokenPurpose.TwoFactorEmailOtp,
            _securityTokenFactory.Hash(code),
            now.AddMinutes(10),
            now);

        _userRepository.Update(user);
        _userRepository.AddSecurityToken(otp);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Auth.TwoFactorEmailSendRequested,
            "Two-factor email resend requested. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
            user.Id,
            otp.ExpiresAt);

        try
        {
            await _emailSender.SendTwoFactorCodeAsync(user.Email, user.FullName, code, otp.ExpiresAt.UtcDateTime, cancellationToken);
            _logger.LogInformation(
                EventIdStore.Auth.TwoFactorEmailSent,
                "Two-factor email resent. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                otp.ExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                EventIdStore.Auth.TwoFactorEmailSendFailed,
                ex,
                "Two-factor email resend failed. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                otp.ExpiresAt);
        }

        return new AuthActionMessageDto("Doğrulama kodu gönderildi.");
    }
}
