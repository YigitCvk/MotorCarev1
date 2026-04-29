using FluentValidation;

namespace MotorCare.Application.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(v => v.Plate).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Brand).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Model).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Year).InclusiveBetween(1900, DateTime.Now.Year + 1);
        RuleFor(v => v.ChassisNumber).MaximumLength(100).When(v => !string.IsNullOrEmpty(v.ChassisNumber));
        RuleFor(v => v.EngineNumber).MaximumLength(100).When(v => !string.IsNullOrEmpty(v.EngineNumber));
        RuleFor(v => v.Color).MaximumLength(50).When(v => !string.IsNullOrEmpty(v.Color));
        RuleFor(v => v.CurrentKm).GreaterThanOrEqualTo(0).When(v => v.CurrentKm.HasValue);
        RuleFor(v => v.CurrentCustomerId).NotEqual(Guid.Empty).When(v => v.CurrentCustomerId.HasValue);
    }
}
