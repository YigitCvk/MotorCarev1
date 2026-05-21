using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.AddOperationToOrder;

public class AddOperationToOrderCommandValidator : AbstractValidator<AddOperationToOrderCommand>
{
    public AddOperationToOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().WithMessage("İşçilik adı zorunludur.").MaximumLength(500);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("İşçilik miktarı sıfırdan büyük olmalıdır.");
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("İşçilik birim fiyatı negatif olamaz.");
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).WithMessage("İşçilik indirimi negatif olamaz.");
        RuleFor(x => x.Discount)
            .LessThanOrEqualTo(x => x.Quantity * x.UnitPrice)
            .WithMessage("İşçilik indirimi satır toplamından büyük olamaz.");
        RuleFor(x => x.Notes).MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
