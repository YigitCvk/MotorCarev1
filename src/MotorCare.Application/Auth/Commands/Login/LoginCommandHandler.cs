using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        ILogger<LoginCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
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

    internal static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
