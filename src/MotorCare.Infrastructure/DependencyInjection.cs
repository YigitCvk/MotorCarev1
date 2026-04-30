using MotorCare.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MotorCare.Infrastructure.Persistence;
using MotorCare.Infrastructure.Persistence.Repositories;
using MotorCare.Infrastructure.Persistence.Seed;
using MotorCare.Infrastructure.Email;
using MotorCare.Infrastructure.Services;
using MotorCare.Infrastructure.Security;
using MotorCare.Infrastructure.Tenancy;
using MotorCare.Domain.Repositories;

namespace MotorCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, HeaderTenantProvider>();
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IMotorcycleModelCatalogRepository, MotorcycleModelCatalogRepository>();
        services.AddScoped<MotorcycleModelCatalogSeeder>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        services.AddScoped<IConsumableCatalogRepository, ConsumableCatalogRepository>();
        services.AddScoped<IServiceCatalogRepository, ServiceCatalogRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IMotorcycleInspectionRepository, MotorcycleInspectionRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<ISecurityTokenFactory, SecurityTokenFactory>();
        services.AddScoped<IAuthLinkBuilder, AuthLinkBuilder>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<LoggingEmailSender>();
        services.AddScoped<IEmailSender>(sp =>
        {
            var env = sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailOptions>>().Value;
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("MotorCare.Infrastructure.Email");

            if (!options.SendEmails)
            {
                logger.LogInformation("Email sending disabled by configuration. Falling back to logging email sender.");
                return sp.GetRequiredService<LoggingEmailSender>();
            }

            var smtpConfigured =
                string.Equals(options.Provider, "Smtp", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(options.SmtpHost) &&
                !string.IsNullOrWhiteSpace(options.FromEmail);

            if (smtpConfigured)
            {
                return sp.GetRequiredService<SmtpEmailSender>();
            }

            if (env.IsDevelopment() || env.IsStaging())
            {
                logger.LogWarning("SMTP configuration is incomplete in {Environment}. Falling back to logging email sender.", env.EnvironmentName);
                return sp.GetRequiredService<LoggingEmailSender>();
            }

            logger.LogError("SMTP configuration is incomplete in {Environment}. Falling back to logging email sender.", env.EnvironmentName);
            return sp.GetRequiredService<LoggingEmailSender>();
        });
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        return services;
    }
}
