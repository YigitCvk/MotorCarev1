namespace MotorCare.Application.Common.Interfaces;

public interface IAuthLinkBuilder
{
    string BuildEmailVerificationUrl(string email, string token);
    string BuildPasswordResetUrl(string email, string token);
}
