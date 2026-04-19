namespace MotorCare.App.Models.Appointments;

public class CreateAppointmentRequest
{
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Plate { get; set; }
    public AppointmentType Type { get; set; } = AppointmentType.Maintenance;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public string? Note { get; set; }
    public string? Complaint { get; set; }
}
