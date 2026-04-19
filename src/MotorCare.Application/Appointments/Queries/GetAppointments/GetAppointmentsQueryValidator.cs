using FluentValidation;

namespace MotorCare.Application.Appointments.Queries.GetAppointments;

public sealed class GetAppointmentsQueryValidator : AbstractValidator<GetAppointmentsQuery>
{
    public GetAppointmentsQueryValidator()
    {
        RuleFor(x => x.EndTo)
            .Must((query, endTo) => !endTo.HasValue || !query.StartFrom.HasValue || endTo.Value >= query.StartFrom.Value)
            .WithMessage("EndTo must be greater than or equal to StartFrom.");
    }
}
