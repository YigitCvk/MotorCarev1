using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public sealed class ConsumableCatalogItemConfiguration : IEntityTypeConfiguration<ConsumableCatalogItem>
{
    public void Configure(EntityTypeBuilder<ConsumableCatalogItem> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .HasMaxLength(50);

        builder.Property(c => c.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.SubCategory)
            .HasMaxLength(50);

        builder.Property(c => c.Brand)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ProductName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Specification)
            .HasMaxLength(100);

        builder.Property(c => c.Notes)
            .HasMaxLength(250);

        builder.HasIndex(c => new { c.TenantId, c.Category, c.Brand, c.ProductName }).IsUnique();

        // Database seeding
        SeedDefaults(builder);
    }

    private static void SeedDefaults(EntityTypeBuilder<ConsumableCatalogItem> builder)
    {
        // 1. Motor Yağı
        builder.HasData(CreateDefault("Motor Yağı", "4T Tam Sentetik", "Motul", "7100 10W-40 4T", "API SP, JASO MA2", "Günlük kullanım / performans / street"));
        builder.HasData(CreateDefault("Motor Yağı", "4T Yarı Sentetik", "Motul", "5100 10W-40 4T", "API SM, JASO MA2", "Standart kullanım"));
        builder.HasData(CreateDefault("Motor Yağı", "4T Tam Sentetik", "Castrol", "Power1 Racing 10W-40", "API SN, JASO MA2", "Performans serisi"));

        // 2. Yağ Filtresi
        builder.HasData(CreateDefault("Yağ Filtresi", "Standart", "Hiflofiltro", "HF138", null, "Sık kullanılan model"));
        builder.HasData(CreateDefault("Yağ Filtresi", "Standart", "Hiflofiltro", "HF204", null, "Sık kullanılan model"));
        builder.HasData(CreateDefault("Yağ Filtresi", "Orijinal", "Yamaha", "OEM Yağ Filtresi", null, "OEM"));

        // 3. Hava Filtresi
        builder.HasData(CreateDefault("Hava Filtresi", "Standart", "Hiflofiltro", "HFA4922", null, "MT-09 / Tracer 900"));
        builder.HasData(CreateDefault("Hava Filtresi", "Performans", "K&N", "YA-9015", null, "Yıkanabilir"));

        // 4. Buji
        builder.HasData(CreateDefault("Buji", "İridyum", "NGK", "CR9EIX", null, "Uzun ömürlü performans"));
        builder.HasData(CreateDefault("Buji", "Standart", "NGK", "CR9E", null, "Standart OEM muadili"));
        builder.HasData(CreateDefault("Buji", "Lazer İridyum", "NGK", "LMAR8A-9", null, "Honda / Yamaha yeni nesil"));

        // 5. Zincir
        builder.HasData(CreateDefault("Zincir", "O-Ring / X-Ring", "DID", "520VX3", "118 Bakla", "Street / Enduro"));
        builder.HasData(CreateDefault("Zincir", "X-Ring", "DID", "525VX3", "120 Bakla", "Touring / Sport"));

        // 6. Ön/Arka Dişli
        builder.HasData(CreateDefault("Ön/Arka Dişli", "Ön Dişli", "JT Sprockets", "JTF520.15", "15 Diş", null));
        builder.HasData(CreateDefault("Ön/Arka Dişli", "Arka Dişli", "JT Sprockets", "JTR856.45", "45 Diş", null));

        // 7. Zincir + Dişli Set
        builder.HasData(CreateDefault("Zincir + Dişli Seti", "Performans Set", "DID", "Zincir Dişli Seti (520VX3)", null, "Set halinde değişim"));

        // 8. Fren Balatası
        builder.HasData(CreateDefault("Fren Balatası", "Sinterli (Ön)", "EBC", "FA252HH", null, "Yüksek performanslı frenleme"));
        builder.HasData(CreateDefault("Fren Balatası", "Organik (Arka)", "EBC", "FA174", null, "Standart kullanım"));
        builder.HasData(CreateDefault("Fren Balatası", "Sinterli (Ön)", "Brembo", "07YA23SA", null, "OEM Kalitesinde Sinterli"));

        // 9. Fren Diski
        builder.HasData(CreateDefault("Fren Diski", "Ön Disk", "EBC", "MD Serisi", null, "OEM Muadili"));
        builder.HasData(CreateDefault("Fren Diski", "Arka Disk", "Brembo", "Serie Oro", null, "Performans diski"));

        // 10. Fren Hidroliği
        builder.HasData(CreateDefault("Fren Hidroliği", "DOT 4", "Motul", "DOT 4 LV", "DOT 4", "ABS uyumlu, düşük viskoziteli"));
        builder.HasData(CreateDefault("Fren Hidroliği", "DOT 5.1", "Motul", "DOT 5.1", "DOT 5.1", "Yüksek performans / Track"));

        // 11. Soğutma Sıvısı
        builder.HasData(CreateDefault("Soğutma Sıvısı", "Hazır Karışım", "Motul", "Motocool Factory Line", "Organik", "Alüminyum radyatör dostu"));
        builder.HasData(CreateDefault("Soğutma Sıvısı", "Hazır Karışım", "Castrol", "Radicool SF", "Organik", "Uzun ömürlü antifriz"));

        // 12. Fork Yağı
        builder.HasData(CreateDefault("Fork Yağı", "10W (Medium)", "Motul", "Fork Oil Expert 10W", "Technosynthese", "Standart süspansiyon tepkisi"));
        builder.HasData(CreateDefault("Fork Yağı", "15W (Medium-Heavy)", "Motul", "Fork Oil Expert 15W", "Technosynthese", "Daha sert süspansiyon tepkisi"));

        // 13. Akü
        builder.HasData(CreateDefault("Akü", "AGM/MF", "Yuasa", "YTZ10S", "12V 8.6Ah", "Bakımsız AGM Akü"));
        builder.HasData(CreateDefault("Akü", "Lityum İyon", "BS Battery", "BSLI-04", "12V", "Hafif ve yüksek marş gücü"));
    }

    private static ConsumableCatalogItem CreateDefault(
        string category,
        string? subCategory,
        string brand,
        string productName,
        string? specification,
        string? notes)
    {
        var item = new ConsumableCatalogItem(
            null, // tenantId
            category,
            brand,
            productName,
            subCategory,
            specification,
            notes,
            true // isSystemDefault
        );

        // Reset tracking fields required for EF HasData (Id must be stable across migrations)
        // We will generate deterministic Guids for seeds based on their unique attributes
        var uniqueString = $"{category}-{brand}-{productName}".ToLowerInvariant();
        var seedGuid = CreateDeterministicGuid(uniqueString);
        
        // EF Core requires setting private properties in HasData using reflection or anonymous types if public setters aren't available.
        // We will use an anonymous object instead inside HasData, but since ConsumableCatalogItem only has private setters, 
        // we can use a workaround or make internal setters. Wait, let's use reflection to set ID or just change setters to 'internal' for EF core.
        // Actually, `HasData` with entities that don't have public setters is tricky. Let's return anonymous types.
        // Wait, EF Core 8 `HasData` allows using the Entity itself, and it maps by convention. But the constructor generates random Guid. 
        // I need to override the Guid.
        item.GetType().GetProperty("Id")!.SetValue(item, seedGuid);
        
        return item;
    }

    private static Guid CreateDeterministicGuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
