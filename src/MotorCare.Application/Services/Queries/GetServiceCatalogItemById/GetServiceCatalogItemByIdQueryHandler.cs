using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Services.Queries.GetServiceCatalogItemById;

public sealed class GetServiceCatalogItemByIdQueryHandler : IRequestHandler<GetServiceCatalogItemByIdQuery, ServiceCatalogItemDto?>
{
    private readonly IServiceCatalogRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetServiceCatalogItemByIdQueryHandler(IServiceCatalogRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<ServiceCatalogItemDto?> Handle(GetServiceCatalogItemByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var item = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        return new ServiceCatalogItemDto(
            item.Id,
            item.Name,
            item.Category,
            ServiceCategoryTextMapper.ToText(item.Category),
            item.Description,
            item.DefaultDurationMinutes,
            item.DefaultPrice,
            item.IsActive);
    }
}
