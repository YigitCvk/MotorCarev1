using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Appointments.Queries.GetAppointments;

public sealed record GetAppointmentsQuery(
    DateTimeOffset? StartFrom,
    DateTimeOffset? EndTo,
    AppointmentStatus? Status,
    AppointmentType? Type,
    string? SearchText) : IRequest<IReadOnlyList<AppointmentDto>>;
