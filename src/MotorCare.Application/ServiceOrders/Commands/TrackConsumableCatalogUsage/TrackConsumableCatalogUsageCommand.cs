using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.ServiceOrders.Commands.TrackConsumableCatalogUsage;

public sealed record TrackConsumableCatalogUsageCommand(IReadOnlyList<ConsumableCatalogItemInput> Items) : IRequest;

public sealed class TrackConsumableCatalogUsageCommandHandler : IRequestHandler<TrackConsumableCatalogUsageCommand>
{
    private readonly IConsumableCatalogRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<TrackConsumableCatalogUsageCommandHandler> _logger;

    public TrackConsumableCatalogUsageCommandHandler(
        IConsumableCatalogRepository repository,
        ITenantProvider tenantProvider,
        ILogger<TrackConsumableCatalogUsageCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(TrackConsumableCatalogUsageCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            _logger.LogWarning("Skipping consumable catalog tracking: No active tenant context.");
            return;
        }

        var uniqueInputs = request.Items
            .Where(x => !string.IsNullOrWhiteSpace(x.Category) && 
                        !string.IsNullOrWhiteSpace(x.Brand) && 
                        !string.IsNullOrWhiteSpace(x.ProductName))
            .GroupBy(item => new { 
                Category = item.Category.Trim(), 
                Brand = item.Brand.Trim(), 
                ProductName = item.ProductName.Trim() 
            })
            .Select(g => g.First())
            .ToList();

        if (uniqueInputs.Count == 0)
        {
            return;
        }

        foreach (var input in uniqueInputs)
        {
            try
            {
                var existing = await _repository.GetExactMatchAsync(
                    input.Category, 
                    input.Brand, 
                    input.ProductName, 
                    cancellationToken);

                if (existing is not null)
                {
                    existing.IncrementUsage();
                    _repository.Update(existing);
                }
                else
                {
                    var newItem = new ConsumableCatalogItem(
                        tenantId,
                        input.Category,
                        input.Brand,
                        input.ProductName,
                        input.SubCategory,
                        input.Specification,
                        input.Notes,
                        isSystemDefault: false);

                    newItem.IncrementUsage();
                    await _repository.AddAsync(newItem, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to track consumable catalog usage for item {Category} - {ProductName}", input.Category, input.ProductName);
            }
        }
    }
}
