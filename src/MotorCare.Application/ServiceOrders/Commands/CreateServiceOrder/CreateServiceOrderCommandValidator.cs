using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.CreateServiceOrder;

public class CreateServiceOrderCommandValidator : AbstractValidator<CreateServiceOrderCommand>
{
    public CreateServiceOrderCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.VehicleKm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Complaint).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Complaint));
    }
}
