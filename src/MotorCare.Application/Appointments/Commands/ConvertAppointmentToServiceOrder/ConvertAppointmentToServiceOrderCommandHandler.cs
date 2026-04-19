using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.Appointments.Commands.ConvertAppointmentToServiceOrder;

public sealed class ConvertAppointmentToServiceOrderCommandHandler : IRequestHandler<ConvertAppointmentToServiceOrderCommand, Guid>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IOrderNumberGenerator _orderNumberGenerator;

    public ConvertAppointmentToServiceOrderCommandHandler(
        IAppointmentRepository appointmentRepository,
        IServiceOrderRepository serviceOrderRepository,
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        ITenantProvider tenantProvider,
        IOrderNumberGenerator orderNumberGenerator)
    {
        _appointmentRepository = appointmentRepository;
        _serviceOrderRepository = serviceOrderRepository;
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
        _tenantProvider = tenantProvider;
        _orderNumberGenerator = orderNumberGenerator;
    }

    public async Task<Guid> Handle(ConvertAppointmentToServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Appointment", request.Id);

        if (!appointment.CustomerId.HasValue || !appointment.VehicleId.HasValue)
        {
            throw new ConflictException("Appointment must be linked to both customer and vehicle before conversion.");
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(appointment.VehicleId.Value, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Vehicles.Vehicle), appointment.VehicleId.Value);

        var customer = await _customerRepository.GetByIdAsync(appointment.CustomerId.Value, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Customers.Customer), appointment.CustomerId.Value);

        var orderNo = await _orderNumberGenerator.GenerateAsync(tenantId, cancellationToken);
        var existing = await _serviceOrderRepository.GetByOrderNoAsync(tenantId, orderNo, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"A service order with order number '{orderNo}' already exists.");
        }

        var serviceOrder = new ServiceOrder(
            tenantId,
            orderNo,
            vehicle.Id,
            customer.Id,
            request.VehicleKm,
            appointment.Complaint);

        appointment.ConvertToServiceOrder(serviceOrder.Id);

        await _serviceOrderRepository.AddAsync(serviceOrder, cancellationToken);
        _appointmentRepository.Update(appointment);
        await _serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return serviceOrder.Id;
    }
}
