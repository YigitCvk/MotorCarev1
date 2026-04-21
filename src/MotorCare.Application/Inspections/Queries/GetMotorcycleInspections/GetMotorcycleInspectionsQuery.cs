using FluentValidation;
using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inspections.Queries.GetMotorcycleInspections;

public sealed record GetMotorcycleInspectionsQuery(
    string? Q,
    MotorcycleInspectionPackageType? PackageType,
    MotorcycleInspectionStatus? Status,
    Guid? CustomerId,
    Guid? VehicleId,
    DateTimeOffset? CreatedFrom,
    DateTimeOffset? CreatedTo,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<MotorcycleInspectionListItemDto>>;

public sealed class GetMotorcycleInspectionsQueryValidator : AbstractValidator<GetMotorcycleInspectionsQuery>
{
    public GetMotorcycleInspectionsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetMotorcycleInspectionsQueryHandler
    : IRequestHandler<GetMotorcycleInspectionsQuery, PagedResult<MotorcycleInspectionListItemDto>>
{
    private readonly IMotorcycleInspectionRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetMotorcycleInspectionsQueryHandler(
        IMotorcycleInspectionRepository repository,
        ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<PagedResult<MotorcycleInspectionListItemDto>> Handle(GetMotorcycleInspectionsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => request.PageSize
        };

        var (items, totalCount) = await _repository.GetPagedAsync(
            tenantId,
            request.Q,
            request.PackageType,
            request.Status,
            request.CustomerId,
            request.VehicleId,
            request.CreatedFrom,
            request.CreatedTo,
            pageNumber,
            pageSize,
            cancellationToken);

        var resultItems = items.Select(x => new MotorcycleInspectionListItemDto(
            x.Id,
            x.InspectionNo,
            x.CustomerId,
            x.VehicleId,
            x.CustomerName,
            x.Phone,
            x.Plate,
            x.PackageType,
            MotorcycleInspectionTextMapper.ToText(x.PackageType),
            x.Status,
            MotorcycleInspectionTextMapper.ToText(x.Status),
            x.PackagePrice,
            x.CreatedAt,
            x.CompletedAt))
            .ToList();

        return PagedResult<MotorcycleInspectionListItemDto>.Create(resultItems, pageNumber, pageSize, totalCount);
    }
}
