using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;

namespace MotorCare.Domain.Appointments;

public class Appointment : AggregateRoot, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public Guid? CustomerId { get; private set; }
    public Guid? VehicleId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Plate { get; private set; }
    public AppointmentType Type { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTimeOffset StartAt { get; private set; }
    public DateTimeOffset EndAt { get; private set; }
    public string? Note { get; private set; }
    public string? Complaint { get; private set; }
    public Guid? ServiceOrderId { get; private set; }

    private Appointment()
    {
    }

    public Appointment(
        string tenantId,
        Guid? customerId,
        Guid? vehicleId,
        string customerName,
        string phone,
        string? plate,
        AppointmentType type,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        string? note,
        string? complaint)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new DomainException("Tenant ID is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        UpdateDetails(customerId, vehicleId, customerName, phone, plate, type, startAt, endAt, note, complaint);
        Status = AppointmentStatus.Scheduled;
    }

    public void UpdateDetails(
        Guid? customerId,
        Guid? vehicleId,
        string customerName,
        string phone,
        string? plate,
        AppointmentType type,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        string? note,
        string? complaint)
    {
        EnsureCanEdit();
        ValidateTiming(startAt, endAt);

        if (string.IsNullOrWhiteSpace(customerName)) throw new DomainException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(phone)) throw new DomainException("Phone is required.");

        CustomerId = customerId;
        VehicleId = vehicleId;
        CustomerName = customerName.Trim();
        Phone = phone.Trim();
        Plate = string.IsNullOrWhiteSpace(plate) ? null : plate.Trim().ToUpperInvariant();
        Type = type;
        StartAt = startAt.ToUniversalTime();
        EndAt = endAt.ToUniversalTime();
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Complaint = string.IsNullOrWhiteSpace(complaint) ? null : complaint.Trim();
    }

    public void UpdateStatus(AppointmentStatus status)
    {
        if (Status == AppointmentStatus.ConvertedToOrder)
        {
            throw new DomainException("Converted appointments cannot change status.");
        }

        Status = status;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.ConvertedToOrder)
        {
            throw new DomainException("Converted appointments cannot be cancelled.");
        }

        Status = AppointmentStatus.Cancelled;
    }

    public void MarkCheckedIn()
    {
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            throw new DomainException("Cancelled or no-show appointments cannot be checked in.");
        }

        Status = AppointmentStatus.CheckedIn;
    }

    public void ConvertToServiceOrder(Guid serviceOrderId)
    {
        if (serviceOrderId == Guid.Empty) throw new DomainException("Service order ID is required.");
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            throw new DomainException("Cancelled or no-show appointments cannot be converted.");
        }

        if (Status == AppointmentStatus.ConvertedToOrder || ServiceOrderId.HasValue)
        {
            throw new DomainException("Appointment is already converted to a service order.");
        }

        ServiceOrderId = serviceOrderId;
        Status = AppointmentStatus.ConvertedToOrder;
    }

    private void EnsureCanEdit()
    {
        if (Status == AppointmentStatus.ConvertedToOrder)
        {
            throw new DomainException("Converted appointments cannot be updated.");
        }
    }

    private static void ValidateTiming(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (endAt <= startAt)
        {
            throw new DomainException("End date must be greater than start date.");
        }
    }
}
