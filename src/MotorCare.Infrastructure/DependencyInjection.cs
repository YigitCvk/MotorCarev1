using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MotorCare.Domain.Common;
using MotorCare.Infrastructure.Persistence;
using MotorCare.Infrastructure.Tenancy;

namespace MotorCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, HeaderTenantProvider>();
        services.AddScoped<MotorCare.Domain.Repositories.IVehicleRepository, MotorCare.Infrastructure.Persistence.Repositories.VehicleRepository>();

        services.AddDbContext<ApplicationDbContext>(options =>
            // Using In-Memory db for MVP skeleton as no connection string is strictly required yet, or SqlServer
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=MotorCareDb;Trusted_Connection=True;Encrypt=False",
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        return services;
    }
}
