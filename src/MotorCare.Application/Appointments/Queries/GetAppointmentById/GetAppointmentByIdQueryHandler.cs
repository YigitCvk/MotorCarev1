using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Appointments.Queries.GetAppointmentById;

public sealed class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto?>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantProvider _tenantProvider;

    public GetAppointmentByIdQueryHandler(IAppointmentRepository appointmentRepository, ITenantProvider tenantProvider)
    {
        _appointmentRepository = appointmentRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<AppointmentDto?> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, tenantId, cancellationToken);
        if (appointment is null)
        {
            return null;
        }

        return new AppointmentDto(
            appointment.Id,
            appointment.CustomerId,
            appointment.VehicleId,
            appointment.CustomerName,
            appointment.Phone,
            appointment.Plate,
            appointment.Type,
            AppointmentTextMapper.ToText(appointment.Type),
            appointment.Status,
            AppointmentTextMapper.ToText(appointment.Status),
            appointment.StartAt,
            appointment.EndAt,
            appointment.Note,
            appointment.Complaint,
            appointment.ServiceOrderId);
    }
}
