using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, AuthActionMessageDto>
{
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
        var tokenHash = _securityTokenFactory.Hash(request.Token);
        var token = await _userRepository.GetActiveSecurityTokenByHashAsync(tokenHash, UserSecurityTokenPurpose.EmailVerification, cancellationToken)
            ?? throw new UnauthorizedAccessException("Doğrulama bağlantısı geçersiz veya süresi dolmuş.");

        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Doğrulama bağlantısı geçersiz veya süresi dolmuş.");

        if (!string.Equals(user.Email, request.Email.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Doğrulama bağlantısı geçersiz veya süresi dolmuş.");
        }

        user.MarkEmailVerified();
        user.ConsumeSecurityToken(tokenHash, DateTimeOffset.UtcNow);
        user.RevokeSecurityTokens(UserSecurityTokenPurpose.EmailVerification, DateTimeOffset.UtcNow);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthActionMessageDto("E-posta adresiniz doğrulandı.");
    }
}
