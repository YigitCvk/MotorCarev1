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
            AppointmentType.Maintenance => "Bakım",
            AppointmentType.Repair => "Onarım",
            AppointmentType.Cleaning => "Temizlik",
            AppointmentType.Washing => "Yıkama",
            AppointmentType.Inspection => "Ekspertiz",
            AppointmentType.TireChange => "Lastik Değişimi",
            AppointmentType.Other => "Diğer",
            _ => type.ToString()
        };

    public static string ToText(AppointmentStatus status) =>
        status switch
        {
            AppointmentStatus.Scheduled => "Planlandı",
            AppointmentStatus.Confirmed => "Onaylandı",
            AppointmentStatus.CheckedIn => "Servise Alındı",
            AppointmentStatus.ConvertedToOrder => "İş Emrine Dönüştü",
            AppointmentStatus.Cancelled => "İptal",
            AppointmentStatus.NoShow => "Gelmedi",
            AppointmentStatus.Completed => "Tamamlandı",
            _ => status.ToString()
        };
}
