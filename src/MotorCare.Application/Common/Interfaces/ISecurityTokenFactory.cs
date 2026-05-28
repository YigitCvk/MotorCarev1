namespace MotorCare.Application.Common.Interfaces;

public interface ISecurityTokenFactory
{
    string GenerateOpaqueToken();
    string GenerateNumericCode(int digits = 6);
    string Hash(string value);
}
