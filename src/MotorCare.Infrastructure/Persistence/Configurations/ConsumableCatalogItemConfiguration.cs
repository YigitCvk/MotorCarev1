using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public sealed class ConsumableCatalogItemConfiguration : IEntityTypeConfiguration<ConsumableCatalogItem>
{
    private static readonly DateTimeOffset SeedCreatedAt = new(2026, 4, 29, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<ConsumableCatalogItem> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .HasMaxLength(50);

        builder.Property(c => c.Category)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(c => c.SubCategory)
            .HasMaxLength(100);

        builder.Property(c => c.Brand)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(c => c.ProductName)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(c => c.Specification)
            .HasMaxLength(160);

        builder.Property(c => c.Notes)
            .HasMaxLength(250);

        builder.HasIndex(c => new { c.TenantId, c.Category, c.Brand, c.ProductName }).IsUnique();

        builder.HasData(GetSystemDefaults());
    }

    private static object[] GetSystemDefaults()
    {
        return
        [
            Seed("Motor Yağı", "4T Tam Sentetik", "Motul", "7100 10W-40 4T", "API SP, JASO MA2", "Günlük kullanım / performans / street"),
            Seed("Motor Yağı", "4T Tam Sentetik", "Motul", "7100 10W-30 4T", "API SP, JASO MA2", "Düşük viskozite isteyen modeller"),
            Seed("Motor Yağı", "4T Sentetik", "Castrol", "POWER1 4T 10W-40", "POWER1 serisi", "Street / commuter / scooter"),
            Seed("Motor Yağı", "4T Tam Sentetik", "Shell", "Advance Ultra 4T", "Shell Advance Ultra", "Modern 4T motosikletler"),
            Seed("Motor Yağı", "4T Sentetik Teknoloji", "Liqui Moly", "Motorbike 4T 10W-40 Street", "10W-40 Street", "Street / commuter"),
            Seed("Motor Yağı", "4T Mineral / Ekonomik", "Liqui Moly", "10W40 4T Basic Street", "Basic Street", "Ekonomik bakım paketi"),
            Seed("Motor Yağı", "4T Tam Sentetik", "IPONE", "Full Power Katana 10W-40", "PAO + ester / 10W-40", "Roadster / enduro / sportif GT"),

            Seed("Yağ Filtresi", "Spin-on / kartuş", "Hiflofiltro", "HF132", "Örnek referans", "Model bazlı değişir"),
            Seed("Yağ Filtresi", "Spin-on / kartuş", "Hiflofiltro", "HF204", "Örnek referans", "Model bazlı değişir"),
            Seed("Yağ Filtresi", "Spin-on / kartuş", "Hiflofiltro", "HF303", "Örnek referans", "Model bazlı değişir"),
            Seed("Yağ Filtresi", "Performance", "K&N", "KN-204", "Örnek referans", "Model bazlı değişir"),
            Seed("Yağ Filtresi", "Performance", "K&N", "KN-171B", "Örnek referans", "Harley vb. bazı modeller"),
            Seed("Yağ Filtresi", "Performance", "K&N", "KN-164", "Örnek referans", "Model bazlı değişir"),

            Seed("Hava Filtresi", "OEM replacement", "Hiflofiltro", "HFA1134", "Örnek referans", "Model bazlı değişir"),
            Seed("Hava Filtresi", "OEM replacement", "Hiflofiltro", "HFA1304", "Örnek referans", "Örn. Forza 250 sınıfı uygulamalar"),
            Seed("Hava Filtresi", "OEM replacement", "Hiflofiltro", "HFA4301", "Örnek referans", "Örn. XMAX 250/300 sınıfı uygulamalar"),
            Seed("Hava Filtresi", "OEM replacement", "Hiflofiltro", "HFA6303", "Örnek referans", "Örn. KTM 390 Duke sınıfı uygulamalar"),
            Seed("Hava Filtresi", "Yıkanabilir / performance", "K&N", "KT-1113", "Örnek referans", "KTM uygulamaları dahil performans tipi"),
            Seed("Hava Filtresi", "Yıkanabilir / performance", "K&N", "BM-1204", "Örnek referans", "BMW uygulamaları dahil performance tipi"),

            Seed("Buji", "Standart nikel", "NGK", "CR7HSA", "Örnek referans", "Cub / commuter / scooter sınıfında yaygın"),
            Seed("Buji", "Standart nikel", "NGK", "CPR8EA-9", "Örnek referans", "Scooter / street uygulamaları"),
            Seed("Buji", "İridyum", "NGK", "CR9EIX", "Örnek referans", "Daha uzun ömür / performans"),
            Seed("Buji", "Standart nikel", "Denso", "U24ESR-N", "Örnek referans", "Model bazlı değişir"),
            Seed("Buji", "Standart nikel", "Denso", "X24ESR-U", "Örnek referans", "Model bazlı değişir"),

            Seed("Zincir", "X-Ring", "DID", "520VX3", "520 pitch / VX3", "Street + off-road upgrade"),
            Seed("Zincir", "X-Ring", "DID", "525VX3", "525 pitch / VX3", "Orta-üst segment street / touring"),
            Seed("Zincir", "XW-Ring", "RK", "530 ZXW / GXW serisi", "XW-Ring", "Big-cc / road race / superbike"),

            Seed("Ön Dişli", "Çelik", "JT Sprockets", "JTF1586.16RB", "Örnek referans", "Model bazlı değişir"),
            Seed("Arka Dişli", "Çelik", "JT Sprockets", "JTR300.46", "Örnek referans", "Model bazlı değişir"),
            Seed("Zincir + Dişli Set", "Komple kit", "DID + JT", "Did 428D + JT set", "Örnek kombinasyon", "125cc commuter / sport commuter"),

            Seed("Fren Balatası", "Sinter / ön-arka varyantlı", "EBC", "FA197", "Örnek referans", "Model bazlı değişir"),
            Seed("Fren Balatası", "Organik", "EBC", "SFA197", "Örnek referans", "Scooter / street arka uygulamalar"),
            Seed("Fren Balatası", "Sinter", "Brembo", "SA Pads", "SA bileşimi", "All-road / ön balata tarafında yaygın"),
            Seed("Fren Balatası", "Seramik / street", "SBS", "230HM", "Örnek referans", "Scooter / street arka uygulamalar"),
            Seed("Fren Balatası", "Seramik / street", "SBS", "155HM", "Örnek referans", "Scooter / street arka uygulamalar"),

            Seed("Fren Diski", "Round / fixed / floating", "NG Brakes", "Genel seri", "Modele göre referans değişir", "Ön / arka disk"),
            Seed("Fren Diski", "Serie Oro", "Brembo", "Serie Oro", "Fixed / floating seçenekleri", "Premium replacement"),

            Seed("Fren Hidroliği", "DOT 3/4", "Motul", "DOT 3&4 Brake Fluid", "0.5L", "Standart bakım / fren & debriyaj hidrolik sistemleri"),
            Seed("Fren Hidroliği", "DOT 4 LV", "Motul", "DOT 4 LV", "Class 6 / düşük viskozite", "ABS/ESP uyumlu sistemler dahil"),
            Seed("Fren Hidroliği", "Racing", "Motul", "RBF 660 Factory Line", "0.5L", "Yüksek sıcaklık / performans"),
            Seed("Fren Hidroliği", "DOT 4", "Liqui Moly", "Brake Fluid DOT 4 (3093)", "500 ml", "Standart bakım"),
            Seed("Fren Hidroliği", "DOT 4 düşük viskozite", "Liqui Moly", "Brake Fluid SL6 DOT 4 (21167)", "500 ml", "ABS'li sistemlerde tercih edilebilir"),

            Seed("Soğutma Sıvısı", "Hazır kullanım", "Motul", "Motocool Factory Line -35C", "1L / Organic+", "Sıvı soğutmalı motosikletler"),
            Seed("Soğutma Sıvısı", "Hazır kullanım", "Motorex", "Coolant M3.0", "1L / OAT ready to use", "Alüminyum motorlarda yaygın premium seçenek"),

            Seed("Fork Yağı", "Suspension oil", "Motul", "Fork Oil Expert 5W", "5W / 1L", "Ön maşa bakımında"),
            Seed("Fork Yağı", "Suspension oil", "Motul", "Fork Oil Expert 10W", "10W / 1L", "Ön maşa bakımında"),
            Seed("Fork Yağı", "Suspension oil", "Motul", "Fork Oil Expert 20W", "20W / 1L", "Ağır sönümleme isteyen uygulamalar"),
            Seed("Fork Yağı", "Suspension oil", "Motorex", "Racing Fork Oil 5W", "5W / 1L", "Ön maşa bakımında premium seçenek"),

            Seed("Akü", "AGM / MF", "Yuasa", "YTX7L-BS", "12V 6Ah 100 CCA", "Scooter / 125-300cc sınıfında sık görülür"),
            Seed("Akü", "AGM / MF", "Yuasa", "YTX12-BS", "12V 10Ah 180 CCA", "Orta segment street / touring"),
            Seed("Akü", "AGM / MF", "Yuasa", "YTX14-BS", "12V 12.6Ah", "Orta-üst segment"),
            Seed("Akü", "AGM / MF", "Yuasa", "YTZ10S", "12V 9.1Ah 190 CCA", "Sport / naked / scooter uygulamaları"),
            Seed("Akü", "SLA / MF", "BS Battery", "BTX7L-BS", "12V 6.3Ah 100A EN", "125-300cc sınıfında yaygın alternatif"),
            Seed("Akü", "SLA / MF", "BS Battery", "BTZ10S", "12V 9Ah 190A EN", "Orta segment street / touring")
        ];
    }

    private static object Seed(
        string category,
        string? subCategory,
        string brand,
        string productName,
        string? specification,
        string? notes)
    {
        return new
        {
            Id = CreateDeterministicGuid($"{category}|{brand}|{productName}"),
            TenantId = (string?)null,
            Category = category,
            SubCategory = subCategory,
            Brand = brand,
            ProductName = productName,
            Specification = specification,
            Notes = notes,
            IsSystemDefault = true,
            IsActive = true,
            UsageCount = 0,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = (DateTimeOffset?)null
        };
    }

    private static Guid CreateDeterministicGuid(string input)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input.ToLowerInvariant()));
        return new Guid(hash);
    }
}
