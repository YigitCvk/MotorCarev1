using MotorCare.Infrastructure.Email;

namespace MotorCare.Application.UnitTests.Auth;

public class EmailVerificationTemplateTests
{
    [Fact]
    public void CreateVerificationEmail_ContainsCode()
    {
        var message = EmailTemplateFactory.CreateVerificationEmail(
            "test@example.com", "Test User", "987654", DateTime.UtcNow.AddMinutes(10));

        message.HtmlBody.Should().Contain("987654");
        message.TextBody.Should().Contain("987654");
    }

    [Fact]
    public void CreateVerificationEmail_DoesNotContainVerificationUrl()
    {
        var message = EmailTemplateFactory.CreateVerificationEmail(
            "test@example.com", "Test User", "987654", DateTime.UtcNow.AddMinutes(10));

        message.HtmlBody.Should().NotContain("verify-email?token=");
        message.HtmlBody.Should().NotContain("verificationUrl");
        message.TextBody.Should().NotContain("verify-email?token=");
    }

    [Fact]
    public void CreateVerificationEmail_ContainsExpiryMinutes()
    {
        var message = EmailTemplateFactory.CreateVerificationEmail(
            "test@example.com", "Test User", "987654", DateTime.UtcNow.AddMinutes(10));

        message.HtmlBody.Should().Contain("dakika");
    }

    [Fact]
    public void CreateVerificationEmail_SubjectContainsDogrulamaKodu()
    {
        var message = EmailTemplateFactory.CreateVerificationEmail(
            "test@example.com", "Test User", "987654", DateTime.UtcNow.AddMinutes(10));

        message.Subject.Should().Contain("doğrulama kodu");
    }
}
