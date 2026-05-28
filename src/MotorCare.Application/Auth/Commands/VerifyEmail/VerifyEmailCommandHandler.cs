using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, AuthActionMessageDto>
{
    private const string InvalidCodeMessage = "Doğrulama kodu geçersiz veya süresi dolmuş.";

    private readonly IUserRepository _userRepository;
    private readonly ISecurityTokenFactory _securityTokenFactory;

    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        ISecurityTokenFactory securityTokenFactory)
    {
        _userRepository = userRepository;
        _securityTokenFactory = securityTokenFactory;
    }

    public async Task<AuthActionMessageDto> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedTenant = request.TenantIdentifier.Trim().ToLowerInvariant();

        // Load user with security tokens so RevokeSecurityTokens can traverse the collection.
        var user = await _userRepository.GetByEmailWithSecurityTokensAsync(
            normalizedTenant, normalizedEmail, cancellationToken)
            ?? throw new UnauthorizedAccessException(InvalidCodeMessage);

        if (user.IsEmailVerified)
            return new AuthActionMessageDto("E-posta adresiniz zaten doğrulanmış.");

        var now = DateTimeOffset.UtcNow;
        var codeHash = _securityTokenFactory.Hash(request.Code);

        // Get the user's own active token — avoids cross-user hash collision for 6-digit codes.
        var latest = await _userRepository.GetLatestActiveSecurityTokenAsync(
            user.Id, UserSecurityTokenPurpose.EmailVerification, cancellationToken);

        if (latest is null)
            throw new UnauthorizedAccessException(InvalidCodeMessage);

        if (!string.Equals(latest.TokenHash, codeHash, StringComparison.Ordinal))
        {
            latest.RegisterFailedAttempt(now);
            await _userRepository.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException(InvalidCodeMessage);
        }

        user.MarkEmailVerified();
        latest.Consume(now);
        user.RevokeSecurityTokens(UserSecurityTokenPurpose.EmailVerification, now);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthActionMessageDto("E-posta adresiniz doğrulandı.");
    }
}
