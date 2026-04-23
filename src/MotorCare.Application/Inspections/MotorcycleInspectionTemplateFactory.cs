using MotorCare.Domain.Enums;
using MotorCare.Domain.Inspections;

namespace MotorCare.Application.Inspections;

public static class MotorcycleInspectionTemplateFactory
{
    public static IReadOnlyList<MotorcycleInspectionItemTemplate> Create(MotorcycleInspectionPackageType packageType)
    {
        var items = new List<MotorcycleInspectionItemTemplate>();

        if (packageType is MotorcycleInspectionPackageType.MechanicalAndRunningGear or MotorcycleInspectionPackageType.Full)
        {
            items.AddRange(CreateCategory(
                MotorcycleInspectionCategory.MechanicalAndRunningGear,
                [
                    "Ön Takım Bağlantı Elemanları",
                    "Gidon-Mesnet Bağlantı Elemanları",
                    "Gidon Rulmanı Deformasyon",
                    "Ön Süspansiyon Grubu",
                    "Arka Süspansiyon Grubu",
                    "Ön Jant Darbe - Deformasyon",
                    "Arka Jant Darbe - Deformasyon",
                    "Fren Balataları Ön",
                    "Fren Diskleri",
                    "Fren Balataları Arka",
                    "Zincir - Kayış",
                    "Dişli",
                    "Ön Lastik",
                    "Arka Lastik",
                    "Motor Yağı",
                    "Soğutma Sıvısı",
                    "Akü",
                    "Elcik ve Manetlerin Durumu",
                    "Anahtar-Kilit Mekanizmaları",
                    "Aydınlatmalar-Korna-Tuş Takımları",
                    "Elektronik Destekler (ABS, Traction, Mode vb.)",
                    "Soğutma Sistemi-Fan",
                    "Orta Sehpa-Yan Sehpa",
                    "Motor Yağ Kaçağı Durumu",
                    "Egzoz Sistemi"
                ]));

            items.AddRange(CreateCategory(
                MotorcycleInspectionCategory.TestRide,
                [
                    "Test Sürüşü"
                ]));
        }

        if (packageType is MotorcycleInspectionPackageType.BodyAndFairing or MotorcycleInspectionPackageType.Full)
        {
            items.AddRange(CreateCategory(
                MotorcycleInspectionCategory.BodyAndFairing,
                [
                    "Ön Grenaj",
                    "Far",
                    "Depo",
                    "Sele",
                    "Sol Yan Grenaj",
                    "Sağ Yan Grenaj",
                    "Arka Grenaj",
                    "Ön Çamurluk",
                    "Arka Çamurluk",
                    "Ayna Sol",
                    "Ayna Sağ",
                    "Egzoz",
                    "Ön Jant",
                    "Arka Jant",
                    "Şasi",
                    "Radyatör Koruma",
                    "Koruma Demiri",
                    "Topcase",
                    "Yan Çanta Sol",
                    "Yan Çanta Sağ",
                    "Yedek Anahtar",
                    "Aksesuar",
                    "Orta Sehpa",
                    "Elcik Isıtma",
                    "Elcik Koruma",
                    "Plaka & Plakalık",
                    "Çamur Sıyırıcı & Çamurluk",
                    "Koruma Demiri & Takozu",
                    "Çakar Sinyal",
                    "Harici Aydınlatma & Sis Farı",
                    "Aynalar",
                    "Gidon Amortisörü",
                    "Gidon Yükseltme",
                    "Ön Cam & Tur Camı",
                    "Egzoz Koruma",
                    "Diğer Aksesuarlar",
                    "Hasarlı Parça",
                    "Boyalı Parça"
                ]));
        }

        if (packageType is MotorcycleInspectionPackageType.ObdAndElectrical or MotorcycleInspectionPackageType.Full)
        {
            items.AddRange(CreateCategory(
                MotorcycleInspectionCategory.ObdAndElectrical,
                [
                    "Kısa Far",
                    "Uzun Far",
                    "Fren Lambaları",
                    "Gösterge Aydınlatmaları",
                    "Sinyaller",
                    "Korna",
                    "ABS",
                    "Çekiş Kontrol",
                    "Sürüş Modları",
                    "Elektronik Destekler",
                    "Anahtarsız Çalıştırma",
                    "Fan",
                    "Vites Göstergesi",
                    "Plaka Aydınlatması",
                    "Yakıt Göstergesi",
                    "OBD Test",
                    "Arıza 1",
                    "Arıza 2",
                    "Arıza 3",
                    "Arıza 4"
                ]));
        }

        items.Add(new MotorcycleInspectionItemTemplate(
            MotorcycleInspectionCategory.General,
            "Genel Açıklama",
            items.Count + 1));

        return items;
    }

    private static IEnumerable<MotorcycleInspectionItemTemplate> CreateCategory(
        MotorcycleInspectionCategory category,
        IReadOnlyList<string> names)
    {
        for (var i = 0; i < names.Count; i++)
        {
            yield return new MotorcycleInspectionItemTemplate(category, names[i], i + 1);
        }
    }
}
