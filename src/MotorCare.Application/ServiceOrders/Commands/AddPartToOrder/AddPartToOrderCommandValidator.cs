using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.AddPartToOrder;

public class AddPartToOrderCommandValidator : AbstractValidator<AddPartToOrderCommand>
{
    public AddPartToOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PartName).NotEmpty().WithMessage("Parça adı zorunludur.").MaximumLength(200);
        RuleFor(x => x.PartNumber).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.PartNumber));
        RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("Parça birim fiyatı sıfırdan büyük olmalıdır.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Parça miktarı sıfırdan büyük olmalıdır.");
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).WithMessage("Parça indirimi negatif olamaz.");
        RuleFor(x => x.Discount)
            .LessThanOrEqualTo(x => x.Quantity * x.UnitPrice)
            .WithMessage("Parça indirimi satır toplamından büyük olamaz.");
        RuleFor(x => x.Notes).MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
