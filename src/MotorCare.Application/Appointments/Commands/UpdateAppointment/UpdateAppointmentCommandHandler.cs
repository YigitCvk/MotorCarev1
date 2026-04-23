using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Appointments.Commands.UpdateAppointment;

public sealed class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ILogger<UpdateAppointmentCommandHandler> _logger;

    public UpdateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        ITenantProvider tenantProvider,
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        ILogger<UpdateAppointmentCommandHandler> logger)
    {
        _appointmentRepository = appointmentRepository;
        _tenantProvider = tenantProvider;
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
        _logger = logger;
    }

    public async Task<AppointmentDto> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId.Value, tenantId, cancellationToken);
            if (customer is null)
            {
                throw new UnauthorizedAccessException("Customer was not found for the current tenant.");
            }
        }

        if (request.VehicleId.HasValue)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value, tenantId, cancellationToken);
            if (vehicle is null)
            {
                throw new UnauthorizedAccessException("Vehicle was not found for the current tenant.");
            }
        }

        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Appointment", request.Id);

        appointment.UpdateDetails(
            request.CustomerId,
            request.VehicleId,
            request.CustomerName,
            request.Phone,
            request.Plate,
            request.Type,
            request.StartAt,
            request.EndAt,
            request.Note,
            request.Complaint);

        _appointmentRepository.Update(appointment);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Appointment.AppointmentUpdated,
            "Appointment {AppointmentId} updated for tenant {TenantId}. CustomerId={CustomerId} VehicleId={VehicleId} StartAt={StartAt} EndAt={EndAt}",
            appointment.Id,
            tenantId,
            appointment.CustomerId,
            appointment.VehicleId,
            appointment.StartAt,
            appointment.EndAt);

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
