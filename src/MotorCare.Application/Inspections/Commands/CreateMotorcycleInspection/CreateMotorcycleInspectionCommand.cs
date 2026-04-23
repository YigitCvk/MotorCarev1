using FluentValidation;
using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Inspections;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inspections.Commands.CreateMotorcycleInspection;

public sealed record CreateMotorcycleInspectionCommand(
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
    Domain.Enums.MotorcycleInspectionPackageType PackageType,
    string? GeneralNotes,
    string? TestRideNotes,
    string? CosmeticNotes) : IRequest<Guid>;

public sealed class CreateMotorcycleInspectionCommandValidator : AbstractValidator<CreateMotorcycleInspectionCommand>
{
    public CreateMotorcycleInspectionCommandValidator()
    {
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
        RuleFor(x => x.Year).GreaterThanOrEqualTo(1900).When(x => x.Year.HasValue);
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue);
    }
}

public sealed class CreateMotorcycleInspectionCommandHandler : IRequestHandler<CreateMotorcycleInspectionCommand, Guid>
{
    private static readonly string[] TurkeyTimeZoneIds = ["Turkey Standard Time", "Europe/Istanbul"];
    private readonly IMotorcycleInspectionRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public CreateMotorcycleInspectionCommandHandler(
        IMotorcycleInspectionRepository repository,
        ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateMotorcycleInspectionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var turkeyTimeZone = ResolveTurkeyTimeZone();
        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, turkeyTimeZone);
        var localDayStart = new DateTimeOffset(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0, localNow.Offset);
        var localDayEnd = localDayStart.AddDays(1);
        var dayStartUtc = localDayStart.ToUniversalTime();
        var dayEndUtc = localDayEnd.ToUniversalTime();

        var count = await _repository.GetTodayCountAsync(tenantId, dayStartUtc, dayEndUtc, cancellationToken);

        string inspectionNo;
        do
        {
            count++;
            inspectionNo = $"EXP-{localNow:yyyyMMdd}-{count:000}";
        }
        while (await _repository.GetByInspectionNoAsync(tenantId, inspectionNo, cancellationToken) is not null);

        var templates = MotorcycleInspectionTemplateFactory.Create(request.PackageType);

        var inspection = new MotorcycleInspection(
            tenantId,
            inspectionNo,
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
            templates);

        await _repository.AddAsync(inspection, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return inspection.Id;
    }

    private static TimeZoneInfo ResolveTurkeyTimeZone()
    {
        foreach (var timeZoneId in TurkeyTimeZoneIds)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}
