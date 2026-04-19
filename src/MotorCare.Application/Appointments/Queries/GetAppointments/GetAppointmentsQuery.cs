using MediatR;
using MotorCare.Application.Common.Models;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Appointments.Queries.GetAppointments;

public sealed record GetAppointmentsQuery(
    DateTimeOffset? StartFrom,
    DateTimeOffset? EndTo,
    AppointmentStatus? Status,
    AppointmentType? Type,
    string? SearchText,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<AppointmentDto>>;
