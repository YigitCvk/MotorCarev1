using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Appointments.Commands.UpdateAppointmentStatus;

public sealed class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, Unit>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateAppointmentStatusCommandHandler> _logger;

    public UpdateAppointmentStatusCommandHandler(
        IAppointmentRepository appointmentRepository,
        ITenantProvider tenantProvider,
        ILogger<UpdateAppointmentStatusCommandHandler> logger)
    {
        _appointmentRepository = appointmentRepository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Appointment", request.Id);

        appointment.UpdateStatus(request.Status);

        _appointmentRepository.Update(appointment);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Appointment.AppointmentStatusUpdated,
            "Appointment status updated. AppointmentId={AppointmentId} NewStatus={NewStatus}",
            request.Id,
            request.Status);

        return Unit.Value;
    }
}
