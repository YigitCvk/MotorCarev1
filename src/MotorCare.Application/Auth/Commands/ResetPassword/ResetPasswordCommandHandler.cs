using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthActionMessageDto>
{
    private const string InvalidCodeMessage = "Şifre sıfırlama kodu geçersiz veya süresi dolmuş.";

    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecurityTokenFactory _securityTokenFactory;

    public ResetPasswordCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISecurityTokenFactory securityTokenFactory)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _securityTokenFactory = securityTokenFactory;
    }

    public async Task<AuthActionMessageDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tenantIdentifier = request.TenantIdentifier.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var tenant = await _tenantRepository.GetByIdentifierAsync(tenantIdentifier, cancellationToken);
        if (tenant is null || !tenant.IsActive)
        {
            throw new UnauthorizedAccessException(InvalidCodeMessage);
        }

        var emailUser = await _userRepository.GetByEmailWithSecurityTokensAsync(tenant.Identifier, normalizedEmail, cancellationToken);
        if (emailUser is null || !emailUser.IsActive)
        {
            throw new UnauthorizedAccessException(InvalidCodeMessage);
        }

        var now = DateTimeOffset.UtcNow;
        var codeHash = _securityTokenFactory.Hash(request.Code);
        var latest = await _userRepository.GetLatestActiveSecurityTokenAsync(
            emailUser.Id,
            UserSecurityTokenPurpose.PasswordReset,
            cancellationToken);

        if (latest is null)
        {
            throw new UnauthorizedAccessException(InvalidCodeMessage);
        }

        if (!string.Equals(latest.TokenHash, codeHash, StringComparison.Ordinal))
        {
            latest.RegisterFailedAttempt(now);
            await _userRepository.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException(InvalidCodeMessage);
        }

        var user = await _userRepository.GetByIdWithRefreshTokensAsync(latest.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException(InvalidCodeMessage);

        if (!string.Equals(user.TenantId, tenant.Identifier, StringComparison.Ordinal) ||
            !string.Equals(user.Email, normalizedEmail, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException(InvalidCodeMessage);
        }

        if (_passwordHasher.Verify(user.PasswordHash, request.NewPassword))
        {
            throw new InvalidOperationException("Yeni şifre eski şifrenizle aynı olamaz.");
        }

        user.ChangePasswordHash(_passwordHasher.Hash(request.NewPassword));
        latest.Consume(now);
        emailUser.RevokeSecurityTokens(UserSecurityTokenPurpose.PasswordReset, now);
        user.RevokeActiveRefreshTokens(now);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthActionMessageDto("Şifreniz başarıyla güncellendi.");
    }
}
