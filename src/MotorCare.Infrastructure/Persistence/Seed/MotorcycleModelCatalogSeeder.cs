using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Domain.Vehicles;

namespace MotorCare.Infrastructure.Persistence.Seed;

public sealed class MotorcycleModelCatalogSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MotorcycleModelCatalogSeeder> _logger;

    public MotorcycleModelCatalogSeeder(ApplicationDbContext dbContext, ILogger<MotorcycleModelCatalogSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return;
            }

            var existing = await _dbContext.MotorcycleModelCatalogItems
                .AsNoTracking()
                .Select(x => new { x.BrandNormalized, x.ModelNormalized })
                .ToListAsync(cancellationToken);

            var existingKeys = existing
                .Select(x => $"{x.BrandNormalized}|{x.ModelNormalized}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var inserted = 0;
            var skipped = 0;

            foreach (var item in MotorcycleModelCatalogDefaults.Items)
            {
                var key = $"{item.Brand.Trim().ToUpperInvariant()}|{item.Model.Trim().ToUpperInvariant()}";
                if (existingKeys.Contains(key))
                {
                    skipped++;
                    continue;
                }

                await _dbContext.MotorcycleModelCatalogItems.AddAsync(
                    new MotorcycleModelCatalogItem(
                        item.Brand,
                        item.Model,
                        item.ModelFamily,
                        item.Segment,
                        item.EngineCc,
                        item.OriginCountry,
                        item.OriginRegion),
                    cancellationToken);

                existingKeys.Add(key);
                inserted++;
            }

            if (inserted > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    EventIdStore.Vehicle.MotorcycleCatalogSeeded,
                    "Motorcycle catalog seeded. InsertedCount={InsertedCount} SkippedCount={SkippedCount}",
                    inserted,
                    skipped);
            }
            else
            {
                _logger.LogInformation(
                    EventIdStore.Vehicle.MotorcycleCatalogDuplicateSkipped,
                    "Motorcycle catalog seed skipped. InsertedCount=0 SkippedCount={SkippedCount}",
                    skipped);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                EventIdStore.Vehicle.MotorcycleCatalogSearchError,
                ex,
                "Motorcycle catalog seed failed.");
        }
    }
}
