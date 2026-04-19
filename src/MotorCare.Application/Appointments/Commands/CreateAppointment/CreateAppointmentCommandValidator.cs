using FluentValidation;

namespace MotorCare.Application.Appointments.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Plate).MaximumLength(20);
        RuleFor(x => x.Note).MaximumLength(1000);
        RuleFor(x => x.Complaint).MaximumLength(1000);
        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt)
            .WithMessage("EndAt must be greater than StartAt.");
    }
}
