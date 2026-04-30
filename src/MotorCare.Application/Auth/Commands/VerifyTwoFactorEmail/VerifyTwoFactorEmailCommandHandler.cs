using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.VerifyTwoFactorEmail;

public sealed class VerifyTwoFactorEmailCommandHandler : IRequestHandler<VerifyTwoFactorEmailCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly ISecurityTokenFactory _securityTokenFactory;

    public VerifyTwoFactorEmailCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        ISecurityTokenFactory securityTokenFactory)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _securityTokenFactory = securityTokenFactory;
    }

    public async Task<AuthResponseDto> Handle(VerifyTwoFactorEmailCommand request, CancellationToken cancellationToken)
    {
        var ticketHash = _securityTokenFactory.Hash(request.Ticket);
        var challenge = await _userRepository.GetActiveSecurityTokenByHashAsync(ticketHash, UserSecurityTokenPurpose.TwoFactorChallenge, cancellationToken)
            ?? throw new UnauthorizedAccessException("Doğrulama oturumu geçersiz veya süresi dolmuş.");

        var user = await _userRepository.GetByIdAsync(challenge.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Doğrulama oturumu geçersiz veya süresi dolmuş.");

        var otpHash = _securityTokenFactory.Hash(request.Code);
        var otp = await _userRepository.GetActiveSecurityTokenByHashAsync(otpHash, UserSecurityTokenPurpose.TwoFactorEmailOtp, cancellationToken);
        if (otp is null || otp.UserId != user.Id)
        {
            throw new UnauthorizedAccessException("Doğrulama kodu geçersiz.");
        }

        var tenant = await _tenantRepository.GetByIdentifierAsync(user.TenantId, cancellationToken)
            ?? throw new UnauthorizedAccessException("İşletme bulunamadı.");

        var now = DateTimeOffset.UtcNow;
        user.ConsumeSecurityToken(ticketHash, now);
        user.ConsumeSecurityToken(otpHash, now);

        var refreshToken = _refreshTokenGenerator.Generate();
        user.MarkLogin(now);
        var refreshTokenEntity = user.AddRefreshToken(_securityTokenFactory.Hash(refreshToken), now.AddDays(7), now);

        _userRepository.Update(user);
        _userRepository.AddRefreshToken(refreshTokenEntity);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            _jwtTokenGenerator.GenerateAccessToken(user, tenant),
            refreshToken,
            user.Id,
            tenant.Id.ToString(),
            tenant.Identifier,
            user.Email,
            user.Role.ToString());
    }
}
