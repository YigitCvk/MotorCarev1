using System.Security.Cryptography;
using System.Text;
using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public LoginCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdentifierAsync(request.TenantIdentifier, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid tenant or credentials.");

        if (!tenant.IsActive)
        {
            throw new UnauthorizedAccessException("The tenant is inactive.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(tenant.Identifier, normalizedEmail, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid tenant or credentials.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("The user is inactive.");
        }

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid tenant or credentials.");
        }

        var refreshToken = _refreshTokenGenerator.Generate();
        var now = DateTimeOffset.UtcNow;
        var refreshTokenHash = HashToken(refreshToken);
        user.MarkLogin(now);
        var refreshTokenEntity = user.AddRefreshToken(refreshTokenHash, now.AddDays(7), now);
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

    internal static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
