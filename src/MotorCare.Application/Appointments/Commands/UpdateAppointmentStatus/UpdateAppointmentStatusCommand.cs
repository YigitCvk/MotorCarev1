using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Appointments.Commands.UpdateAppointmentStatus;

public sealed record UpdateAppointmentStatusCommand(Guid Id, AppointmentStatus Status) : IRequest<Unit>;
