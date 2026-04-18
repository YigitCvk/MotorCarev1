using System.Security.Cryptography;
using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Infrastructure.Security;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
