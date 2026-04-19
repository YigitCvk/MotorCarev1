using MediatR;

namespace MotorCare.Application.Appointments.Queries.GetAppointmentById;

public sealed record GetAppointmentByIdQuery(Guid Id) : IRequest<AppointmentDto?>;
