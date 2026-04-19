using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Appointments.Commands.UpdateAppointmentStatus;

public sealed class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, Unit>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantProvider _tenantProvider;

    public UpdateAppointmentStatusCommandHandler(IAppointmentRepository appointmentRepository, ITenantProvider tenantProvider)
    {
        _appointmentRepository = appointmentRepository;
        _tenantProvider = tenantProvider;
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
        return Unit.Value;
    }
}
