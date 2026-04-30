namespace MotorCare.Application.Common.Interfaces;

public interface IAuthLinkBuilder
{
    string BuildEmailVerificationUrl(string email, string token);
}
