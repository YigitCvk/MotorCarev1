namespace MotorCare.App.Models.Appointments;

public enum AppointmentType
{
    Maintenance = 1,
    Repair = 2,
    Cleaning = 3,
    Washing = 4,
    Inspection = 5,
    TireChange = 6,
    Other = 7
}

public enum AppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    CheckedIn = 3,
    ConvertedToOrder = 4,
    Cancelled = 5,
    NoShow = 6,
    Completed = 7
}

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Plate { get; set; }
    public AppointmentType Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public string? Note { get; set; }
    public string? Complaint { get; set; }
    public Guid? ServiceOrderId { get; set; }
}
