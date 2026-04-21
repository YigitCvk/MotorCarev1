using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inspections.Queries.GetMotorcycleInspectionById;

public sealed record GetMotorcycleInspectionByIdQuery(Guid Id) : IRequest<MotorcycleInspectionDto?>;

public sealed class GetMotorcycleInspectionByIdQueryHandler : IRequestHandler<GetMotorcycleInspectionByIdQuery, MotorcycleInspectionDto?>
{
    private readonly IMotorcycleInspectionRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetMotorcycleInspectionByIdQueryHandler(
        IMotorcycleInspectionRepository repository,
        ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<MotorcycleInspectionDto?> Handle(GetMotorcycleInspectionByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var inspection = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken);
        if (inspection is null)
        {
            return null;
        }

        return new MotorcycleInspectionDto(
            inspection.Id,
            inspection.InspectionNo,
            inspection.CustomerId,
            inspection.VehicleId,
            inspection.CustomerName,
            inspection.Phone,
            inspection.Plate,
            inspection.Brand,
            inspection.Model,
            inspection.Year,
            inspection.Mileage,
            inspection.ChassisNumber,
            inspection.EngineNumber,
            inspection.Query5664,
            inspection.MileageQuery,
            inspection.PackageType,
            MotorcycleInspectionTextMapper.ToText(inspection.PackageType),
            inspection.Status,
            MotorcycleInspectionTextMapper.ToText(inspection.Status),
            inspection.PackagePrice,
            inspection.GeneralNotes,
            inspection.TestRideNotes,
            inspection.CosmeticNotes,
            inspection.CreatedAt,
            inspection.UpdatedAt,
            inspection.CompletedAt,
            inspection.Items
                .OrderBy(x => x.Category)
                .ThenBy(x => x.SortOrder)
                .Select(x => new MotorcycleInspectionItemDto(
                    x.Id,
                    x.Category,
                    MotorcycleInspectionTextMapper.ToText(x.Category),
                    x.Name,
                    x.Result,
                    MotorcycleInspectionTextMapper.ToText(x.Result),
                    x.Notes,
                    x.SortOrder))
                .ToList());
    }
}
