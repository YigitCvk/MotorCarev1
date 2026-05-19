using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.AddConsumableToOrder;

public sealed class AddConsumableToOrderCommandValidator : AbstractValidator<AddConsumableToOrderCommand>
{
    public AddConsumableToOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().WithMessage("Sarf kategorisi zorunludur.").MaximumLength(64);
        RuleFor(x => x.ProductName).NotEmpty().WithMessage("Sarf ürün adı zorunludur.").MaximumLength(160);
        RuleFor(x => x.Brand).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.Brand));
        RuleFor(x => x.SubCategory).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.SubCategory));
        RuleFor(x => x.Specification).MaximumLength(160).When(x => !string.IsNullOrWhiteSpace(x.Specification));
        RuleFor(x => x.Notes).MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Sarf miktarı sıfırdan büyük olmalıdır.");
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Sarf birim fiyatı negatif olamaz.");
    }
}
