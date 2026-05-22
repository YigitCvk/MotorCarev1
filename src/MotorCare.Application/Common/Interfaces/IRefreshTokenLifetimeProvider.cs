namespace MotorCare.Application.Common.Interfaces;

public interface IRefreshTokenLifetimeProvider
{
    DateTimeOffset GetExpiresAt(DateTimeOffset issuedAt);
}
