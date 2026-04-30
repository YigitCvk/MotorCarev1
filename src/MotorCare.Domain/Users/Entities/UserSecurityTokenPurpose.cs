namespace MotorCare.Domain.Users.Entities;

public enum UserSecurityTokenPurpose
{
    EmailVerification = 1,
    PasswordReset = 2,
    TwoFactorEmailOtp = 3,
    TwoFactorChallenge = 4,
    TwoFactorEnableEmailOtp = 5,
    TwoFactorDisableEmailOtp = 6
}
