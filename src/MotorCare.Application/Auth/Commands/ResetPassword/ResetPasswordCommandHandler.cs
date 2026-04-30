using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthActionMessageDto>
{
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
        var tokenHash = _securityTokenFactory.Hash(request.Token);
        var token = await _userRepository.GetActiveSecurityTokenByHashAsync(tokenHash, UserSecurityTokenPurpose.PasswordReset, cancellationToken)
            ?? throw new UnauthorizedAccessException("Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş.");

        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş.");

        if (!string.Equals(user.Email, request.Email.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş.");
        }

        user.ChangePasswordHash(_passwordHasher.Hash(request.NewPassword));
        user.ConsumeSecurityToken(tokenHash, DateTimeOffset.UtcNow);
        user.RevokeSecurityTokens(UserSecurityTokenPurpose.PasswordReset, DateTimeOffset.UtcNow);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthActionMessageDto("Şifreniz güncellendi.");
    }
}
