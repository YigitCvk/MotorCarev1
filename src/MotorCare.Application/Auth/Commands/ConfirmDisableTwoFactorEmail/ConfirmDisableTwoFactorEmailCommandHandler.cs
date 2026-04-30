using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.ConfirmDisableTwoFactorEmail;

public sealed class ConfirmDisableTwoFactorEmailCommandHandler : IRequestHandler<ConfirmDisableTwoFactorEmailCommand, AuthActionMessageDto>
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IUserRepository _userRepository;
    private readonly ISecurityTokenFactory _securityTokenFactory;

    public ConfirmDisableTwoFactorEmailCommandHandler(
        ICurrentUserProvider currentUserProvider,
        IUserRepository userRepository,
        ISecurityTokenFactory securityTokenFactory)
    {
        _currentUserProvider = currentUserProvider;
        _userRepository = userRepository;
        _securityTokenFactory = securityTokenFactory;
    }

    public async Task<AuthActionMessageDto> Handle(ConfirmDisableTwoFactorEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        if (!user.TwoFactorEnabled)
        {
            return new AuthActionMessageDto("Iki asamali dogrulama zaten devre disi.");
        }

        var codeHash = _securityTokenFactory.Hash(request.Code);
        var otp = await _userRepository.GetActiveSecurityTokenByHashAsync(codeHash, UserSecurityTokenPurpose.TwoFactorDisableEmailOtp, cancellationToken);
        if (otp is null || otp.UserId != user.Id)
        {
            var latest = await _userRepository.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.TwoFactorDisableEmailOtp, cancellationToken);
            if (latest is not null)
            {
                user.RegisterSecurityTokenFailedAttempt(latest.TokenHash, DateTimeOffset.UtcNow);
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync(cancellationToken);
            }

            throw new UnauthorizedAccessException("Dogrulama kodu gecersiz.");
        }

        user.ConsumeSecurityToken(codeHash, DateTimeOffset.UtcNow);
        user.SetTwoFactor(false, null);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthActionMessageDto("Iki asamali dogrulama devre disi birakildi.");
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
