using MotorCare.Application.Common.Interfaces;
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
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=MotorCareDb;Username=postgres;Password=postgres",
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        return services;
    }
}
