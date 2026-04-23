using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inspections.Commands.CompleteMotorcycleInspection;

public sealed record CompleteMotorcycleInspectionCommand(Guid Id) : IRequest;

public sealed class CompleteMotorcycleInspectionCommandValidator : AbstractValidator<CompleteMotorcycleInspectionCommand>
{
    public CompleteMotorcycleInspectionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class CompleteMotorcycleInspectionCommandHandler : IRequestHandler<CompleteMotorcycleInspectionCommand>
{
    private readonly IMotorcycleInspectionRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CompleteMotorcycleInspectionCommandHandler> _logger;

    public CompleteMotorcycleInspectionCommandHandler(
        IMotorcycleInspectionRepository repository,
        ITenantProvider tenantProvider,
        ILogger<CompleteMotorcycleInspectionCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(CompleteMotorcycleInspectionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var inspection = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Ekspertiz kaydı bulunamadı.");

        inspection.Complete();
        _repository.Update(inspection);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Inspection.InspectionCompleted,
            "Motorcycle inspection completed. InspectionId={InspectionId} InspectionNo={InspectionNo}",
            inspection.Id,
            inspection.InspectionNo);
    }
}
