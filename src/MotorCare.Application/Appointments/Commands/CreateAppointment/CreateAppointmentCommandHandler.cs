using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Appointments;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Appointments.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;

    public CreateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        ITenantProvider tenantProvider,
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _appointmentRepository = appointmentRepository;
        _tenantProvider = tenantProvider;
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
        _logger = logger;
    }

    public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
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

        var appointment = new Appointment(
            tenantId,
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

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Appointment.AppointmentCreated,
            "Appointment created. AppointmentId={AppointmentId} CustomerName={CustomerName} Type={Type} StartAt={StartAt}",
            appointment.Id,
            request.CustomerName,
            request.Type,
            request.StartAt);

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
