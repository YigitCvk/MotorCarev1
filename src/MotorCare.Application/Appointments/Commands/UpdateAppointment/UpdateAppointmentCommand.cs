using MediatR;
using MotorCare.Application.Appointments;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Appointments.Commands.UpdateAppointment;

public sealed record UpdateAppointmentCommand(
    Guid Id,
    Guid? CustomerId,
    Guid? VehicleId,
    string CustomerName,
    string Phone,
    string? Plate,
    AppointmentType Type,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Note,
    string? Complaint) : IRequest<AppointmentDto>;
