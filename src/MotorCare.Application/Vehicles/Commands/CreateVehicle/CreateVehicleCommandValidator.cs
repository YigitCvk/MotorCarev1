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
    }
}
