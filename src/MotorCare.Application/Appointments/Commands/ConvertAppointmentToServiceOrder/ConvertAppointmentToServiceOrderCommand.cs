using MediatR;

namespace MotorCare.Application.Appointments.Commands.ConvertAppointmentToServiceOrder;

public sealed record ConvertAppointmentToServiceOrderCommand(Guid Id, int VehicleKm) : IRequest<Guid>;
