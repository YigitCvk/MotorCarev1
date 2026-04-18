using MotorCare.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MotorCare.Infrastructure.Security;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool Verify(string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
