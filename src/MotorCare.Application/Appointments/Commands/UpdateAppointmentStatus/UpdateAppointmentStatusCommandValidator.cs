using FluentValidation;

namespace MotorCare.Application.Appointments.Commands.UpdateAppointmentStatus;

public sealed class UpdateAppointmentStatusCommandValidator : AbstractValidator<UpdateAppointmentStatusCommand>
{
    public UpdateAppointmentStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
