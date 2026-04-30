using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IEmailSender _emailSender;
    private readonly ISecurityTokenFactory _securityTokenFactory;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IEmailSender emailSender,
        ISecurityTokenFactory securityTokenFactory,
        ILogger<LoginCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _emailSender = emailSender;
        _securityTokenFactory = securityTokenFactory;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            EventIdStore.Auth.LoginAttempt,
            "Login attempt for TenantIdentifier={TenantIdentifier}",
            request.TenantIdentifier);

        var tenant = await _tenantRepository.GetByIdentifierAsync(request.TenantIdentifier, cancellationToken);
        if (tenant is null || !tenant.IsActive)
        {
            _logger.LogWarning(
                EventIdStore.Auth.LoginFailed,
                "Login failed: tenant not found or inactive. TenantIdentifier={TenantIdentifier}",
                request.TenantIdentifier);

            throw new UnauthorizedAccessException("Invalid tenant or credentials.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(tenant.Identifier, normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive)
        {
            _logger.LogWarning(
                EventIdStore.Auth.LoginFailed,
                "Login failed: user not found or inactive. TenantIdentifier={TenantIdentifier}",
                request.TenantIdentifier);

            throw new UnauthorizedAccessException("Invalid tenant or credentials.");
        }

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            _logger.LogWarning(
                EventIdStore.Auth.LoginFailed,
                "Login failed: invalid password. TenantIdentifier={TenantIdentifier} UserId={UserId}",
                request.TenantIdentifier,
                user.Id);

            throw new UnauthorizedAccessException("Invalid tenant or credentials.");
        }

        if (!user.IsEmailVerified)
        {
            _logger.LogWarning(
                EventIdStore.Auth.LoginFailed,
                "Login blocked because email is not verified. TenantIdentifier={TenantIdentifier} UserId={UserId}",
                request.TenantIdentifier,
                user.Id);

            throw new UnauthorizedAccessException("E-posta adresinizi doğrulamanız gerekiyor.");
        }

        if (user.TwoFactorEnabled && user.TwoFactorProvider == TwoFactorProvider.Email)
        {
            return await CreateTwoFactorChallengeAsync(user, tenant, cancellationToken);
        }

        var refreshToken = _refreshTokenGenerator.Generate();
        var now = DateTimeOffset.UtcNow;
        var refreshTokenHash = HashToken(refreshToken);
        user.MarkLogin(now);
        var refreshTokenEntity = user.AddRefreshToken(refreshTokenHash, now.AddDays(7), now);
        _userRepository.Update(user);
        _userRepository.AddRefreshToken(refreshTokenEntity);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Auth.LoginSucceeded,
            "Login succeeded. UserId={UserId} TenantIdentifier={TenantIdentifier} Role={Role}",
            user.Id,
            tenant.Identifier,
            user.Role);

        return new AuthResponseDto(
            _jwtTokenGenerator.GenerateAccessToken(user, tenant),
            refreshToken,
            user.Id,
            tenant.Id.ToString(),
            tenant.Identifier,
            user.Email,
            user.Role.ToString());
    }

    private async Task<AuthResponseDto> CreateTwoFactorChallengeAsync(Domain.Users.User user, Domain.Tenants.Tenant tenant, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var latestOtp = await _userRepository.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.TwoFactorEmailOtp, cancellationToken);
        if (latestOtp is not null && latestOtp.CreatedAt >= now.AddMinutes(-1))
        {
            throw new UnauthorizedAccessException("Yeni doğrulama kodu istemeden önce kısa süre bekleyin.");
        }

        user.RevokeSecurityTokens(UserSecurityTokenPurpose.TwoFactorEmailOtp, now);
        user.RevokeSecurityTokens(UserSecurityTokenPurpose.TwoFactorChallenge, now);

        var ticket = _securityTokenFactory.GenerateOpaqueToken();
        var code = _securityTokenFactory.GenerateNumericCode();
        var challengeExpiresAt = now.AddMinutes(10);
        var codeExpiresAt = now.AddMinutes(10);

        var challenge = user.AddSecurityToken(
            UserSecurityTokenPurpose.TwoFactorChallenge,
            _securityTokenFactory.Hash(ticket),
            challengeExpiresAt,
            now);

        var otp = user.AddSecurityToken(
            UserSecurityTokenPurpose.TwoFactorEmailOtp,
            _securityTokenFactory.Hash(code),
            codeExpiresAt,
            now);

        _userRepository.Update(user);
        _userRepository.AddSecurityToken(challenge);
        _userRepository.AddSecurityToken(otp);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Auth.TwoFactorEmailSendRequested,
            "Two-factor email requested. UserId={UserId} Provider={Provider} ExpiresAtUtc={ExpiresAtUtc}",
            user.Id,
            "Email",
            codeExpiresAt);

        try
        {
            await _emailSender.SendTwoFactorCodeAsync(user.Email, user.FullName, code, codeExpiresAt.UtcDateTime, cancellationToken);
            _logger.LogInformation(
                EventIdStore.Auth.TwoFactorEmailSent,
                "Two-factor email sent. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                codeExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                EventIdStore.Auth.TwoFactorEmailSendFailed,
                ex,
                "Two-factor email send failed. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                codeExpiresAt);

            throw new UnauthorizedAccessException("Doğrulama kodu gönderilemedi. Lütfen tekrar deneyin.");
        }

        return new AuthResponseDto(
            string.Empty,
            string.Empty,
            user.Id,
            tenant.Id.ToString(),
            tenant.Identifier,
            user.Email,
            user.Role.ToString(),
            true,
            ticket,
            challengeExpiresAt);
    }

    internal static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
