using FluentValidation;
using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inspections.Commands.UpdateMotorcycleInspectionItem;

public sealed record UpdateMotorcycleInspectionItemCommand(
    Guid InspectionId,
    Guid ItemId,
    MotorcycleInspectionResult Result,
    string? Notes) : IRequest;

public sealed class UpdateMotorcycleInspectionItemCommandValidator : AbstractValidator<UpdateMotorcycleInspectionItemCommand>
{
    public UpdateMotorcycleInspectionItemCommandValidator()
    {
        RuleFor(x => x.InspectionId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Result).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class UpdateMotorcycleInspectionItemCommandHandler : IRequestHandler<UpdateMotorcycleInspectionItemCommand>
{
    private readonly IMotorcycleInspectionRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public UpdateMotorcycleInspectionItemCommandHandler(
        IMotorcycleInspectionRepository repository,
        ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UpdateMotorcycleInspectionItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var inspection = await _repository.GetByIdAsync(request.InspectionId, tenantId, cancellationToken)
            ?? throw new NotFoundException("Ekspertiz kaydı bulunamadı.");

        inspection.UpdateItem(request.ItemId, request.Result, request.Notes);

        _repository.Update(inspection);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
