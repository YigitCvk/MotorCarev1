using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Auth.Commands.Login;
using MotorCare.Application.Common;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(IUserRepository userRepository, ILogger<LogoutCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = LoginCommandHandler.HashToken(request.RefreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (user.HasActiveRefreshToken(tokenHash, DateTimeOffset.UtcNow))
        {
            user.RevokeRefreshToken(tokenHash, DateTimeOffset.UtcNow);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            EventIdStore.Auth.LogoutSucceeded,
            "Logout succeeded. UserId={UserId}",
            user.Id);

        return Unit.Value;
    }
}
