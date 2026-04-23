using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inspections.Commands.CancelMotorcycleInspection;

public sealed record CancelMotorcycleInspectionCommand(Guid Id) : IRequest;

public sealed class CancelMotorcycleInspectionCommandValidator : AbstractValidator<CancelMotorcycleInspectionCommand>
{
    public CancelMotorcycleInspectionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class CancelMotorcycleInspectionCommandHandler : IRequestHandler<CancelMotorcycleInspectionCommand>
{
    private readonly IMotorcycleInspectionRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CancelMotorcycleInspectionCommandHandler> _logger;

    public CancelMotorcycleInspectionCommandHandler(
        IMotorcycleInspectionRepository repository,
        ITenantProvider tenantProvider,
        ILogger<CancelMotorcycleInspectionCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(CancelMotorcycleInspectionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var inspection = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Ekspertiz kaydı bulunamadı.");

        inspection.Cancel();
        _repository.Update(inspection);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Inspection.InspectionCancelled,
            "Motorcycle inspection cancelled. InspectionId={InspectionId} InspectionNo={InspectionNo}",
            inspection.Id,
            inspection.InspectionNo);
    }
}
