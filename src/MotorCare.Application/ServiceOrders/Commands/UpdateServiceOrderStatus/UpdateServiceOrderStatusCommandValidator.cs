using FluentValidation;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.ServiceOrders.Commands.UpdateServiceOrderStatus;

public class UpdateServiceOrderStatusCommandValidator : AbstractValidator<UpdateServiceOrderStatusCommand>
{
    public UpdateServiceOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(status => status != ServiceOrderStatus.Open)
            .WithMessage("Geçersiz servis emri durumu. Acik durumuna geri dönüş desteklenmiyor.");

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Note))
            .WithMessage("Durum aciklamasi en fazla 500 karakter olabilir.");
    }
}
