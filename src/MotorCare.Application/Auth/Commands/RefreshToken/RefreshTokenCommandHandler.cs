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
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = LoginCommandHandler.HashToken(request.RefreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken)
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

        if (!user.HasActiveRefreshToken(tokenHash, DateTimeOffset.UtcNow))
        {
            throw new UnauthorizedAccessException("Refresh token is no longer active.");
        }

        user.RevokeRefreshToken(tokenHash, DateTimeOffset.UtcNow);

        var newRefreshToken = _refreshTokenGenerator.Generate();
        var newRefreshTokenEntity = user.AddRefreshToken(
            LoginCommandHandler.HashToken(newRefreshToken),
            DateTimeOffset.UtcNow.AddDays(7),
            DateTimeOffset.UtcNow);

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
