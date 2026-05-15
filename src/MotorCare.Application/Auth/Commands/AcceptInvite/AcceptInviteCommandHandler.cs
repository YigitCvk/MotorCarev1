using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.AcceptInvite;

public sealed class AcceptInviteCommandHandler : IRequestHandler<AcceptInviteCommand, AuthActionMessageDto>
{
    private const string InvalidTokenMessage = "Davet bağlantısı geçersiz veya süresi dolmuş.";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecurityTokenFactory _securityTokenFactory;

    public AcceptInviteCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISecurityTokenFactory securityTokenFactory)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _securityTokenFactory = securityTokenFactory;
    }

    public async Task<AuthActionMessageDto> Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _securityTokenFactory.Hash(request.Token.Trim());
        var inviteToken = await _userRepository.GetActiveSecurityTokenByHashAsync(tokenHash, UserSecurityTokenPurpose.UserInvitation, cancellationToken)
            ?? throw new UnauthorizedAccessException(InvalidTokenMessage);

        var now = DateTimeOffset.UtcNow;
        var user = await _userRepository.GetByIdWithSecurityTokensAsync(inviteToken.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException(InvalidTokenMessage);

        var passwordHash = _passwordHasher.Hash(request.Password);
        user.AcceptInvite(request.FullName.Trim(), passwordHash);
        user.MarkEmailVerified();
        inviteToken.Consume(now);
        user.RevokeSecurityTokens(UserSecurityTokenPurpose.UserInvitation, now);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthActionMessageDto("Hesabınız oluşturuldu. Giriş yapabilirsiniz.");
    }
}
