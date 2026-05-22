using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Auth.Commands.Login;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenLifetimeProvider _refreshTokenLifetimeProvider;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenLifetimeProvider refreshTokenLifetimeProvider,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenLifetimeProvider = refreshTokenLifetimeProvider;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = LoginCommandHandler.HashToken(request.RefreshToken);
        var now = DateTimeOffset.UtcNow;
        var userId = await _userRepository.TryRevokeActiveRefreshTokenAsync(tokenHash, now, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("The user is inactive.");
        }

        var tenant = await _tenantRepository.GetByIdentifierAsync(user.TenantId, cancellationToken)
            ?? throw new UnauthorizedAccessException("The tenant was not found.");

        if (!tenant.IsActive)
        {
            throw new UnauthorizedAccessException("The tenant is inactive.");
        }

        var newRefreshToken = _refreshTokenGenerator.Generate();
        var newRefreshTokenEntity = user.AddRefreshToken(
            LoginCommandHandler.HashToken(newRefreshToken),
            _refreshTokenLifetimeProvider.GetExpiresAt(now),
            now);

        _userRepository.Update(user);
        _userRepository.AddRefreshToken(newRefreshTokenEntity);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto(
            _jwtTokenGenerator.GenerateAccessToken(user, tenant),
            newRefreshToken,
            user.Id,
            tenant.Id.ToString(),
            tenant.Identifier,
            user.Email,
            user.Role.ToString());

        _logger.LogInformation(
            EventIdStore.Auth.TokenRefreshed,
            "Token refreshed. UserId={UserId} TenantIdentifier={TenantIdentifier}",
            user.Id,
            tenant.Identifier);

        return response;
    }
}
