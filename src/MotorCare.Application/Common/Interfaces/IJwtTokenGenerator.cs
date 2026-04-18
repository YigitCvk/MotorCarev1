using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;

namespace MotorCare.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, Tenant tenant);
}
