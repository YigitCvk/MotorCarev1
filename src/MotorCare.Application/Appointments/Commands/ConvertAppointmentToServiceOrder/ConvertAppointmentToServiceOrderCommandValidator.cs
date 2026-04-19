using FluentValidation;

namespace MotorCare.Application.Appointments.Commands.ConvertAppointmentToServiceOrder;

public sealed class ConvertAppointmentToServiceOrderCommandValidator : AbstractValidator<ConvertAppointmentToServiceOrderCommand>
{
    public ConvertAppointmentToServiceOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.VehicleKm).GreaterThanOrEqualTo(0);
    }
}
