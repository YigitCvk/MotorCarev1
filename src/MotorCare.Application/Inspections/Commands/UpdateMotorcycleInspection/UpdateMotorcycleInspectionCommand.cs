using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inspections.Commands.UpdateMotorcycleInspection;

public sealed record UpdateMotorcycleInspectionCommand(
    Guid Id,
    Guid? CustomerId,
    Guid? VehicleId,
    string CustomerName,
    string Phone,
    string Plate,
    string? Brand,
    string? Model,
    int? Year,
    int? Mileage,
    string? ChassisNumber,
    string? EngineNumber,
    string? Query5664,
    string? MileageQuery,
    MotorcycleInspectionPackageType PackageType,
    string? GeneralNotes,
    string? TestRideNotes,
    string? CosmeticNotes) : IRequest;

public sealed class UpdateMotorcycleInspectionCommandValidator : AbstractValidator<UpdateMotorcycleInspectionCommand>
{
    public UpdateMotorcycleInspectionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Plate).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Brand).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.ChassisNumber).MaximumLength(100);
        RuleFor(x => x.EngineNumber).MaximumLength(100);
        RuleFor(x => x.Query5664).MaximumLength(250);
        RuleFor(x => x.MileageQuery).MaximumLength(250);
        RuleFor(x => x.GeneralNotes).MaximumLength(4000);
        RuleFor(x => x.TestRideNotes).MaximumLength(4000);
        RuleFor(x => x.CosmeticNotes).MaximumLength(4000);
        RuleFor(x => x.PackageType).IsInEnum();
    }
}

public sealed class UpdateMotorcycleInspectionCommandHandler : IRequestHandler<UpdateMotorcycleInspectionCommand>
{
    private readonly IMotorcycleInspectionRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateMotorcycleInspectionCommandHandler> _logger;

    public UpdateMotorcycleInspectionCommandHandler(
        IMotorcycleInspectionRepository repository,
        ITenantProvider tenantProvider,
        ILogger<UpdateMotorcycleInspectionCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(UpdateMotorcycleInspectionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var inspection = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Ekspertiz kaydi bulunamadi.");

        inspection.UpdateDetails(
            request.CustomerId,
            request.VehicleId,
            request.CustomerName,
            request.Phone,
            request.Plate,
            request.Brand,
            request.Model,
            request.Year,
            request.Mileage,
            request.ChassisNumber,
            request.EngineNumber,
            request.Query5664,
            request.MileageQuery,
            request.PackageType,
            request.GeneralNotes,
            request.TestRideNotes,
            request.CosmeticNotes,
            MotorcycleInspectionTemplateFactory.Create(request.PackageType));

        _repository.Update(inspection);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Inspection.InspectionUpdated,
            "Inspection {InspectionId} updated for tenant {TenantId}. PackageType={PackageType} VehicleId={VehicleId} CustomerId={CustomerId}",
            inspection.Id,
            tenantId,
            inspection.PackageType,
            inspection.VehicleId,
            inspection.CustomerId);
    }
}
