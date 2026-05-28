using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Vehicles;
using MotorCare.Infrastructure.Persistence.Seed;
using Npgsql;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public sealed class MotorcycleModelCatalogRepository : IMotorcycleModelCatalogRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MotorcycleModelCatalogRepository> _logger;

    public MotorcycleModelCatalogRepository(
        ApplicationDbContext dbContext,
        ILogger<MotorcycleModelCatalogRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> SearchBrandsAsync(string? search, int maxResults, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.MotorcycleModelCatalogItems
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x => EF.Functions.ILike(x.Brand, $"%{term}%"));
            }

            return await query
                .GroupBy(x => x.Brand)
                .OrderByDescending(x => x.Sum(y => y.UsageCount))
                .ThenBy(x => x.Key)
                .Select(x => x.Key)
                .Take(maxResults)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ShouldFallback(ex))
        {
            _logger.LogWarning(ex, "Motorcycle catalog brand search fell back to in-memory defaults.");
            return SearchBrandsFromDefaults(search, maxResults);
        }
    }

    public async Task<IReadOnlyList<MotorcycleModelCatalogItem>> SearchModelsAsync(string brand, string? search, int maxResults, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedBrand = brand.Trim().ToUpperInvariant();

            var query = _dbContext.MotorcycleModelCatalogItems
                .AsNoTracking()
                .Where(x => x.IsActive && x.BrandNormalized == normalizedBrand);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x => EF.Functions.ILike(x.Model, $"%{term}%"));
            }

            return await query
                .OrderByDescending(x => x.UsageCount)
                .ThenBy(x => x.Model)
                .Take(maxResults)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ShouldFallback(ex))
        {
            _logger.LogWarning(ex, "Motorcycle catalog model search fell back to in-memory defaults. Brand={Brand}", brand);
            return SearchModelsFromDefaults(brand, search, maxResults);
        }
    }

    public async Task<IReadOnlyList<MotorcycleModelCatalogItem>> SearchAsync(string? queryText, int maxResults, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.MotorcycleModelCatalogItems
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(queryText))
            {
                var term = queryText.Trim();
                query = query.Where(x =>
                    EF.Functions.ILike(x.Brand, $"%{term}%") ||
                    EF.Functions.ILike(x.Model, $"%{term}%"));
            }

            return await query
                .OrderByDescending(x => x.UsageCount)
                .ThenBy(x => x.Brand)
                .ThenBy(x => x.Model)
                .Take(maxResults)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ShouldFallback(ex))
        {
            _logger.LogWarning(ex, "Motorcycle catalog general search fell back to in-memory defaults. Query={Query}", queryText);
            return SearchFromDefaults(queryText, maxResults);
        }
    }

    private static bool ShouldFallback(Exception ex) =>
        ex is InvalidOperationException ||
        ex is DbUpdateException ||
        ex is PostgresException ||
        ex.InnerException is PostgresException;

    private static IReadOnlyList<string> SearchBrandsFromDefaults(string? search, int maxResults)
    {
        var query = MotorcycleModelCatalogDefaults.Items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Brand.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .Select(x => x.Brand)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .Take(maxResults)
            .ToList();
    }

    private static IReadOnlyList<MotorcycleModelCatalogItem> SearchModelsFromDefaults(string brand, string? search, int maxResults)
    {
        var query = MotorcycleModelCatalogDefaults.Items
            .Where(x => string.Equals(x.Brand, brand, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Model.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderBy(x => x.Model)
            .Take(maxResults)
            .Select(MapSeedToEntity)
            .ToList();
    }

    private static IReadOnlyList<MotorcycleModelCatalogItem> SearchFromDefaults(string? queryText, int maxResults)
    {
        var query = MotorcycleModelCatalogDefaults.Items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(queryText))
        {
            var term = queryText.Trim();
            query = query.Where(x =>
                x.Brand.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Model.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderBy(x => x.Brand)
            .ThenBy(x => x.Model)
            .Take(maxResults)
            .Select(MapSeedToEntity)
            .ToList();
    }

    private static MotorcycleModelCatalogItem MapSeedToEntity(MotorcycleModelCatalogSeedItem item) =>
        new(
            item.Brand,
            item.Model,
            item.ModelFamily,
            item.Segment,
            item.EngineCc,
            item.OriginCountry,
            item.OriginRegion);
}
