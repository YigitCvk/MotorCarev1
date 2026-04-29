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
        RuleForEach(x => x.Consumables).ChildRules(consumable =>
        {
            consumable.RuleFor(x => x.Category).NotEmpty().MaximumLength(64);
            consumable.RuleFor(x => x.ProductName).NotEmpty().MaximumLength(160);
            consumable.RuleFor(x => x.Brand).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.Brand));
            consumable.RuleFor(x => x.SubCategory).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.SubCategory));
            consumable.RuleFor(x => x.Specification).MaximumLength(160).When(x => !string.IsNullOrWhiteSpace(x.Specification));
            consumable.RuleFor(x => x.Notes).MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        });
    }
}
