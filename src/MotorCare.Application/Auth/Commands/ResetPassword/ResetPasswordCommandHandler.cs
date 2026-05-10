using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthActionMessageDto>
{
    private const string InvalidCodeMessage = "Şifre sıfırlama kodu geçersiz veya süresi dolmuş.";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecurityTokenFactory _securityTokenFactory;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISecurityTokenFactory securityTokenFactory)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _securityTokenFactory = securityTokenFactory;
    }

    public async Task<AuthActionMessageDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var users = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var codeHash = _securityTokenFactory.Hash(request.Code);

        foreach (var emailUser in users.Where(x => x.IsActive))
        {
            var latest = await _userRepository.GetLatestActiveSecurityTokenAsync(emailUser.Id, UserSecurityTokenPurpose.PasswordReset, cancellationToken);
            if (latest is null)
            {
                continue;
            }

            if (!string.Equals(latest.TokenHash, codeHash, StringComparison.Ordinal))
            {
                latest.RegisterFailedAttempt(now);
                await _userRepository.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedAccessException(InvalidCodeMessage);
            }

            var user = await _userRepository.GetByIdWithRefreshTokensAsync(latest.UserId, cancellationToken)
                ?? throw new UnauthorizedAccessException(InvalidCodeMessage);

            if (_passwordHasher.Verify(user.PasswordHash, request.NewPassword))
            {
                throw new InvalidOperationException("Yeni şifre eski şifrenizle aynı olamaz.");
            }

            user.ChangePasswordHash(_passwordHasher.Hash(request.NewPassword));
            latest.Consume(now);
            user.RevokeSecurityTokens(UserSecurityTokenPurpose.PasswordReset, now);
            user.RevokeActiveRefreshTokens(now);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return new AuthActionMessageDto("Şifreniz başarıyla güncellendi.");
        }

        throw new UnauthorizedAccessException(InvalidCodeMessage);
    }
}
