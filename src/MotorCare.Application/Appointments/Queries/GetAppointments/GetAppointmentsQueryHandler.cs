using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Appointments.Queries.GetAppointments;

public sealed class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantProvider _tenantProvider;

    public GetAppointmentsQueryHandler(IAppointmentRepository appointmentRepository, ITenantProvider tenantProvider)
    {
        _appointmentRepository = appointmentRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var appointments = await _appointmentRepository.GetFilteredAsync(
            tenantId,
            request.StartFrom,
            request.EndTo,
            request.Status,
            request.Type,
            request.SearchText,
            cancellationToken);

        return appointments
            .Select(appointment => new AppointmentDto(
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
                appointment.ServiceOrderId))
            .ToList();
    }
}
