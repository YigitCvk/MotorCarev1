namespace MotorCare.App.Models.Inspections;

public static class MotorcycleBodyMapDefinitions
{
    public const string LeftImagePath = "/images/motorcycle-left.png";
    public const string RightImagePath = "/images/motorcycle-right.png";

    public static readonly IReadOnlyList<MotorcyclePartDefinition> LeftParts =
    [
        new("hand_clutch_left", 1, "Hand clutch / Debriyaj Kolu", MotorcycleViewType.Left, 33.0, 17.2, ["Hand clutch", "Elcik ve Manetlerin Durumu", "Ayna Sol"]),
        new("ignition_switch_left", 2, "Ignition switch / Kontak", MotorcycleViewType.Left, 34.2, 25.5, ["Ignition switch", "Anahtar-Kilit Mekanizmaları"]),
        new("instrument_display_left", 3, "Instrument display / Gösterge", MotorcycleViewType.Left, 30.8, 28.6, ["Instrument display", "Gösterge Aydınlatmaları"]),
        new("frame_left", 4, "Frame / Şasi", MotorcycleViewType.Left, 58.0, 50.0, ["Frame", "Şasi"]),
        new("front_fork_left", 5, "Front fork / Ön Maşa", MotorcycleViewType.Left, 25.2, 49.8, ["Front fork", "Ön Süspansiyon Grubu"]),
        new("front_brakes_left", 6, "Front brakes / Ön Fren", MotorcycleViewType.Left, 18.8, 64.5, ["Front brakes", "Fren Balataları Ön", "Fren Diskleri"]),
        new("wheels_tires_left", 7, "Wheels & tires / Jant ve Lastik", MotorcycleViewType.Left, 14.8, 78.6, ["Wheels & tires", "Ön Jant", "Ön Lastik", "Arka Jant", "Arka Lastik"]),
        new("foot_shifter_left", 8, "Foot shifter / Vites Pedalı", MotorcycleViewType.Left, 58.5, 72.5, ["Foot shifter", "Vites Pedalı", "Vites Göstergesi"]),
        new("transmission_left", 9, "Transmission / Aktarma", MotorcycleViewType.Left, 78.2, 72.8, ["Transmission", "Zincir - Kayış", "Dişli"]),
        new("engine_left", 10, "Engine / Motor", MotorcycleViewType.Left, 47.4, 60.5, ["Engine", "Motor Yağı", "Motor Yağ Kaçağı Durumu"]),
        new("starter_pedal_left", 11, "Starter pedal / Marş Pedalı", MotorcycleViewType.Left, 56.5, 75.8, ["Starter pedal", "Marş Pedalı"]),
        new("suspension_left", 12, "Suspension / Süspansiyon", MotorcycleViewType.Left, 63.8, 56.8, ["Suspension", "Arka Süspansiyon Grubu"]),
        new("rear_brakes_left", 13, "Rear brakes / Arka Fren", MotorcycleViewType.Left, 86.3, 67.8, ["Rear brakes", "Fren Balataları Arka"]),
        new("seat_left", 14, "Seat / Sele", MotorcycleViewType.Left, 67.8, 37.2, ["Seat", "Sele"]),
        new("fuel_tank_left", 15, "Fuel tank / Yakıt Deposu", MotorcycleViewType.Left, 46.5, 31.5, ["Fuel tank", "Depo", "Yakıt Deposu", "Depo Sol"]),
        new("handlebar_left", 16, "Gidon / Kumandalar", MotorcycleViewType.Left, 37.4, 21.6, ["Gidon", "Gidon / Kumandalar", "Gidon-Mesnet Bağlantı Elemanları"])
    ];

    public static readonly IReadOnlyList<MotorcyclePartDefinition> RightParts =
    [
        new("hand_clutch_right", 1, "Hand clutch / Debriyaj Kolu", MotorcycleViewType.Right, 66.8, 17.4, ["Hand clutch", "Elcik ve Manetlerin Durumu", "Ayna Sağ"]),
        new("ignition_switch_right", 2, "Ignition switch / Kontak", MotorcycleViewType.Right, 63.8, 25.5, ["Ignition switch", "Anahtar-Kilit Mekanizmaları"]),
        new("instrument_display_right", 3, "Instrument display / Gösterge", MotorcycleViewType.Right, 68.2, 28.8, ["Instrument display", "Gösterge Aydınlatmaları"]),
        new("frame_right", 4, "Frame / Şasi", MotorcycleViewType.Right, 42.5, 49.8, ["Frame", "Şasi"]),
        new("front_fork_right", 5, "Front fork / Ön Maşa", MotorcycleViewType.Right, 74.6, 49.8, ["Front fork", "Ön Süspansiyon Grubu"]),
        new("front_brakes_right", 6, "Front brakes / Ön Fren", MotorcycleViewType.Right, 82.6, 64.6, ["Front brakes", "Fren Balataları Ön", "Fren Diskleri"]),
        new("wheels_tires_right", 7, "Wheels & tires / Jant ve Lastik", MotorcycleViewType.Right, 83.3, 79.0, ["Wheels & tires", "Ön Jant", "Ön Lastik", "Arka Jant", "Arka Lastik"]),
        new("foot_shifter_right", 8, "Foot shifter / Vites Pedalı", MotorcycleViewType.Right, 39.5, 75.8, ["Foot shifter", "Vites Pedalı", "Vites Göstergesi"]),
        new("transmission_right", 9, "Transmission / Aktarma", MotorcycleViewType.Right, 43.6, 72.0, ["Transmission", "Zincir - Kayış", "Dişli"]),
        new("engine_right", 10, "Engine / Motor", MotorcycleViewType.Right, 53.5, 60.2, ["Engine", "Motor Yağı", "Motor Yağ Kaçağı Durumu"]),
        new("starter_pedal_right", 11, "Starter pedal / Marş Pedalı", MotorcycleViewType.Right, 46.5, 73.8, ["Starter pedal", "Marş Pedalı"]),
        new("suspension_right", 12, "Suspension / Süspansiyon", MotorcycleViewType.Right, 35.5, 56.6, ["Suspension", "Arka Süspansiyon Grubu"]),
        new("rear_brakes_right", 13, "Rear brakes / Arka Fren", MotorcycleViewType.Right, 16.2, 67.8, ["Rear brakes", "Fren Balataları Arka"]),
        new("seat_right", 14, "Seat / Sele", MotorcycleViewType.Right, 32.8, 37.6, ["Seat", "Sele"]),
        new("fuel_tank_right", 15, "Fuel tank / Yakıt Deposu", MotorcycleViewType.Right, 50.8, 31.3, ["Fuel tank", "Depo", "Yakıt Deposu", "Depo Sağ"]),
        new("handlebar_right", 16, "Gidon / Kumandalar", MotorcycleViewType.Right, 62.5, 21.2, ["Gidon", "Gidon / Kumandalar", "Gidon-Mesnet Bağlantı Elemanları"])
    ];

    public static readonly IReadOnlyList<(MotorcyclePartStatus Status, string Label)> StatusOptions =
    [
        (MotorcyclePartStatus.Original, "Orijinal"),
        (MotorcyclePartStatus.Painted, "Boyalı"),
        (MotorcyclePartStatus.Replaced, "Değişen"),
        (MotorcyclePartStatus.Damaged, "Hasarlı"),
        (MotorcyclePartStatus.Scratched, "Çizik")
    ];

    public static IReadOnlyList<MotorcyclePartDefinition> GetParts(MotorcycleViewType viewType)
        => viewType == MotorcycleViewType.Left ? LeftParts : RightParts;

    public static string GetImagePath(MotorcycleViewType viewType)
        => viewType == MotorcycleViewType.Left ? LeftImagePath : RightImagePath;

    public static string GetStatusText(MotorcyclePartStatus status) => status switch
    {
        MotorcyclePartStatus.Original => "Orijinal",
        MotorcyclePartStatus.Painted => "Boyalı",
        MotorcyclePartStatus.Replaced => "Değişen",
        MotorcyclePartStatus.Damaged => "Hasarlı",
        MotorcyclePartStatus.Scratched => "Çizik",
        MotorcyclePartStatus.Unmarked => "İşaretsiz",
        _ => status.ToString()
    };

    public static string GetStatusColor(MotorcyclePartStatus status) => status switch
    {
        MotorcyclePartStatus.Original => "#16a34a",
        MotorcyclePartStatus.Painted => "#2563eb",
        MotorcyclePartStatus.Replaced => "#f97316",
        MotorcyclePartStatus.Damaged => "#dc2626",
        MotorcyclePartStatus.Scratched => "#f59e0b",
        MotorcyclePartStatus.Unmarked => "#cbd5e1",
        _ => "#cbd5e1"
    };

    public static string GetStatusSoftColor(MotorcyclePartStatus status) => status switch
    {
        MotorcyclePartStatus.Original => "#dcfce7",
        MotorcyclePartStatus.Painted => "#dbeafe",
        MotorcyclePartStatus.Replaced => "#ffedd5",
        MotorcyclePartStatus.Damaged => "#fee2e2",
        MotorcyclePartStatus.Scratched => "#fef3c7",
        MotorcyclePartStatus.Unmarked => "#f8fafc",
        _ => "#f8fafc"
    };
}
