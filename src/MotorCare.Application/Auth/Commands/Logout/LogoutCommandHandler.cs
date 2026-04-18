using MediatR;
using MotorCare.Application.Auth.Commands.Login;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public LogoutCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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

        return Unit.Value;
    }
}
