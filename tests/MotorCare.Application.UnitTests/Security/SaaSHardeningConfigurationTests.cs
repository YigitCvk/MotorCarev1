using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MotorCare.Infrastructure.Email;
using MotorCare.Infrastructure.Security;
using MotorCare.Infrastructure.Tenancy;

namespace MotorCare.Application.UnitTests.Security;

public class SaaSHardeningConfigurationTests
{
    [Fact]
    public void HeaderTenantProvider_IgnoresHeaderFallback_InProductionEvenWhenConfigured()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "tenant-from-header";
        var provider = CreateTenantProvider(httpContext, "Production", allowHeaderFallback: true);

        var tenantId = provider.GetTenantId();

        tenantId.Should().BeNull();
    }

    [Fact]
    public void HeaderTenantProvider_UsesHeaderFallback_InDevelopmentWhenConfigured()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "tenant-from-header";
        var provider = CreateTenantProvider(httpContext, "Development", allowHeaderFallback: true);

        var tenantId = provider.GetTenantId();

        tenantId.Should().Be("tenant-from-header");
    }

    [Fact]
    public void HeaderTenantProvider_PrefersAuthenticatedTenantClaim()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(JwtTokenGenerator.TenantIdentifierClaim, "tenant-from-claim") },
                authenticationType: "Test"))
        };
        httpContext.Request.Headers["X-Tenant-Id"] = "tenant-from-header";
        var provider = CreateTenantProvider(httpContext, "Production", allowHeaderFallback: false);

        var tenantId = provider.GetTenantId();

        tenantId.Should().Be("tenant-from-claim");
    }

    [Fact]
    public void JwtOptions_Throws_WhenSigningKeyIsMissingOrTooShort()
    {
        var options = new JwtOptions
        {
            Issuer = "MotorCare",
            Audience = "MotorCare.Client",
            Key = "short",
            AccessTokenMinutes = 60,
            RefreshTokenDays = 7
        };

        var act = () => JwtOptions.ThrowIfInvalid(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Key*32 bytes*");
    }

    [Fact]
    public void JwtOptions_DoesNotThrow_WhenConfigurationIsValid()
    {
        var options = new JwtOptions
        {
            Issuer = "MotorCare",
            Audience = "MotorCare.Client",
            Key = "12345678901234567890123456789012",
            AccessTokenMinutes = 60,
            RefreshTokenDays = 7
        };

        var act = () => JwtOptions.ThrowIfInvalid(options);

        act.Should().NotThrow();
    }

    [Fact]
    public void EmailOptions_ThrowsInProduction_WhenSmtpConfigurationIsIncomplete()
    {
        var options = new EmailOptions
        {
            Provider = "Smtp",
            FromEmail = "ops@example.com",
            SmtpHost = "",
            SmtpPort = 587,
            SmtpUsername = "smtp-user",
            SmtpPassword = "smtp-password",
            AppBaseUrl = "https://garajpass.com",
            SendEmails = true
        };

        var act = () => EmailOptions.ThrowIfInvalidForEnvironment(options, "Production");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Email:SmtpHost*");
    }

    [Fact]
    public void EmailOptions_AllowsIncompleteSmtpConfigurationOutsideProduction()
    {
        var options = new EmailOptions
        {
            Provider = "Smtp",
            FromEmail = "",
            SmtpHost = "",
            SendEmails = false
        };

        var act = () => EmailOptions.ThrowIfInvalidForEnvironment(options, "Development");

        act.Should().NotThrow();
    }

    private static HeaderTenantProvider CreateTenantProvider(
        HttpContext httpContext,
        string environmentName,
        bool allowHeaderFallback)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tenancy:AllowHeaderTenantFallback"] = allowHeaderFallback.ToString()
            })
            .Build();

        return new HeaderTenantProvider(accessor, configuration, environment);
    }
}
