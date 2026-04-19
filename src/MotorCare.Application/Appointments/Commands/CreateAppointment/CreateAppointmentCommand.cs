using MediatR;
using MotorCare.Application.Appointments;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Appointments.Commands.CreateAppointment;

public sealed record CreateAppointmentCommand(
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
