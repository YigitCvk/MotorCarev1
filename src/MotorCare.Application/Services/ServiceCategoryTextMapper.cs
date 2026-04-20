using MotorCare.Domain.Enums;

namespace MotorCare.Application.Services;

public static class ServiceCategoryTextMapper
{
    public static string ToText(ServiceCategory category) => category switch
    {
        ServiceCategory.PeriodicMaintenance => "Periyodik Bakım",
        ServiceCategory.FaultDiagnosis => "Arıza Tespit",
        ServiceCategory.CarWash => "Oto Yıkama",
        ServiceCategory.Detailing => "Detailing",
        ServiceCategory.TireService => "Lastik",
        ServiceCategory.MotorcycleMaintenance => "Motosiklet Bakım",
        ServiceCategory.BodyPaint => "Kaporta / Boya",
        _ => "Diğer"
    };
}
