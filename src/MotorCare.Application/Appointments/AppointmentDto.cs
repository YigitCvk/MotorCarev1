using MotorCare.Domain.Enums;

namespace MotorCare.Application.Appointments;

public sealed record AppointmentDto(
    Guid Id,
    Guid? CustomerId,
    Guid? VehicleId,
    string CustomerName,
    string Phone,
    string? Plate,
    AppointmentType Type,
    string TypeText,
    AppointmentStatus Status,
    string StatusText,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Note,
    string? Complaint,
    Guid? ServiceOrderId);

public static class AppointmentTextMapper
{
    public static string ToText(AppointmentType type) =>
        type switch
        {
            AppointmentType.Maintenance => "Maintenance",
            AppointmentType.Repair => "Repair",
            AppointmentType.Cleaning => "Cleaning",
            AppointmentType.Washing => "Washing",
            AppointmentType.Inspection => "Inspection",
            AppointmentType.TireChange => "Tire Change",
            AppointmentType.Other => "Other",
            _ => type.ToString()
        };

    public static string ToText(AppointmentStatus status) =>
        status switch
        {
            AppointmentStatus.Scheduled => "Scheduled",
            AppointmentStatus.Confirmed => "Confirmed",
            AppointmentStatus.CheckedIn => "Checked In",
            AppointmentStatus.ConvertedToOrder => "Converted To Order",
            AppointmentStatus.Cancelled => "Cancelled",
            AppointmentStatus.NoShow => "No Show",
            AppointmentStatus.Completed => "Completed",
            _ => status.ToString()
        };
}
