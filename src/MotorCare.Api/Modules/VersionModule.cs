using Carter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MotorCare.Api.Configuration;
using MotorCare.Infrastructure.Persistence;

namespace MotorCare.Api.Modules;

public sealed class VersionModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/version", async (
            IOptions<BuildInfoOptions> buildInfo,
            IWebHostEnvironment environment,
            ApplicationDbContext dbContext,
            ILogger<VersionModule> logger,
            CancellationToken ct) =>
        {
            var latestAppliedMigration = "unavailable";

            try
            {
                var migrations = await dbContext.Database.GetAppliedMigrationsAsync(ct);
                latestAppliedMigration = migrations.LastOrDefault() ?? "none";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to read latest applied migration for version endpoint.");
            }

            var providerName = dbContext.Database.ProviderName switch
            {
                "Npgsql.EntityFrameworkCore.PostgreSQL" => "PostgreSQL",
                null or "" => "unknown",
                _ => dbContext.Database.ProviderName
            };

            return Results.Ok(new
            {
                application = "MotorCare.Api",
                environment = environment.EnvironmentName,
                commit = Normalize(buildInfo.Value.CommitSha),
                buildTime = Normalize(buildInfo.Value.BuildTime),
                machineName = Environment.MachineName,
                databaseProvider = providerName,
                latestAppliedMigration
            });
        })
        .WithTags("Health")
        .WithName("GetVersion")
        .WithOpenApi()
        .Produces(StatusCodes.Status200OK);
    }

    private static string Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
}
