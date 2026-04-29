using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MotorCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumableCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumableCatalogItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Brand = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Specification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsumableSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableSuggestions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ConsumableCatalogItems",
                columns: new[] { "Id", "Brand", "Category", "CreatedAt", "IsActive", "IsSystemDefault", "Notes", "ProductName", "Specification", "SubCategory", "TenantId", "UpdatedAt", "UsageCount" },
                values: new object[,]
                {
                    { new Guid("0e0d3a70-d057-28c9-a58a-4a9259794ce6"), "Motul", "Fren Hidroliği", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "ABS uyumlu, düşük viskoziteli", "DOT 4 LV", "DOT 4", "DOT 4", null, null, 0 },
                    { new Guid("144dd497-8ee6-7fa7-71b0-3fc88bbbc0be"), "NGK", "Buji", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Uzun ömürlü performans", "CR9EIX", null, "İridyum", null, null, 0 },
                    { new Guid("176f59e6-b1d6-bd62-7a9c-500c0e6113ca"), "Motul", "Soğutma Sıvısı", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Alüminyum radyatör dostu", "Motocool Factory Line", "Organik", "Hazır Karışım", null, null, 0 },
                    { new Guid("1ad8db7b-e41a-5d7b-8d34-f33366c89154"), "JT Sprockets", "Ön/Arka Dişli", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, null, "JTR856.45", "45 Diş", "Arka Dişli", null, null, 0 },
                    { new Guid("25fce551-8ff4-39ee-9f08-0237bd1b726d"), "Yuasa", "Akü", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Bakımsız AGM Akü", "YTZ10S", "12V 8.6Ah", "AGM/MF", null, null, 0 },
                    { new Guid("3657f151-ffdb-21a6-f4e9-6c8b7e2db3a0"), "Hiflofiltro", "Hava Filtresi", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "MT-09 / Tracer 900", "HFA4922", null, "Standart", null, null, 0 },
                    { new Guid("4c23f151-df44-38da-99a1-3d88a49b81fe"), "EBC", "Fren Balatası", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Standart kullanım", "FA174", null, "Organik (Arka)", null, null, 0 },
                    { new Guid("5e047430-f336-a816-6e3a-8abf1c726e7b"), "EBC", "Fren Diski", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "OEM Muadili", "MD Serisi", null, "Ön Disk", null, null, 0 },
                    { new Guid("602d9914-2dac-c3a9-6875-ae26126349eb"), "Motul", "Fork Yağı", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Standart süspansiyon tepkisi", "Fork Oil Expert 10W", "Technosynthese", "10W (Medium)", null, null, 0 },
                    { new Guid("67ad2600-961e-1c0a-6fae-9bf7528f985e"), "DID", "Zincir + Dişli Seti", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Set halinde değişim", "Zincir Dişli Seti (520VX3)", null, "Performans Set", null, null, 0 },
                    { new Guid("6f4a08c5-cedf-4d19-5ebb-8d6086f819cd"), "Brembo", "Fren Diski", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Performans diski", "Serie Oro", null, "Arka Disk", null, null, 0 },
                    { new Guid("6f7949a6-c8dc-b7eb-cd93-f18e831126ac"), "Yamaha", "Yağ Filtresi", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "OEM", "OEM Yağ Filtresi", null, "Orijinal", null, null, 0 },
                    { new Guid("70f0e37c-5a0c-c60d-ceeb-877faefc7cd2"), "Motul", "Fork Yağı", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Daha sert süspansiyon tepkisi", "Fork Oil Expert 15W", "Technosynthese", "15W (Medium-Heavy)", null, null, 0 },
                    { new Guid("715216fe-59bb-c2a9-c2ac-f81a4627b88e"), "Castrol", "Motor Yağı", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Performans serisi", "Power1 Racing 10W-40", "API SN, JASO MA2", "4T Tam Sentetik", null, null, 0 },
                    { new Guid("8a8cc886-0952-5bbf-6b02-2e0d54e2d5fd"), "Hiflofiltro", "Yağ Filtresi", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Sık kullanılan model", "HF204", null, "Standart", null, null, 0 },
                    { new Guid("8be6fb80-a059-fe30-fad6-630fa787bbd6"), "NGK", "Buji", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Standart OEM muadili", "CR9E", null, "Standart", null, null, 0 },
                    { new Guid("8fcbb815-85b1-b050-a792-7552c370deef"), "NGK", "Buji", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Honda / Yamaha yeni nesil", "LMAR8A-9", null, "Lazer İridyum", null, null, 0 },
                    { new Guid("aa2ec53c-e352-75a8-8fee-3bcaa8db5a30"), "Hiflofiltro", "Yağ Filtresi", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Sık kullanılan model", "HF138", null, "Standart", null, null, 0 },
                    { new Guid("b15a7733-9b78-f202-18b5-746c667f2b1e"), "DID", "Zincir", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Street / Enduro", "520VX3", "118 Bakla", "O-Ring / X-Ring", null, null, 0 },
                    { new Guid("b241b9ca-d2cc-ece5-8496-8265f8eb5ea8"), "Motul", "Motor Yağı", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Günlük kullanım / performans / street", "7100 10W-40 4T", "API SP, JASO MA2", "4T Tam Sentetik", null, null, 0 },
                    { new Guid("b332bc91-58f1-fd39-858e-e51c9814bfa0"), "Motul", "Fren Hidroliği", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Yüksek performans / Track", "DOT 5.1", "DOT 5.1", "DOT 5.1", null, null, 0 },
                    { new Guid("b5da6d70-39fe-b127-5a64-d3953000617b"), "Brembo", "Fren Balatası", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "OEM Kalitesinde Sinterli", "07YA23SA", null, "Sinterli (Ön)", null, null, 0 },
                    { new Guid("b866391a-2aec-8160-6c45-6b97ab203665"), "BS Battery", "Akü", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Hafif ve yüksek marş gücü", "BSLI-04", "12V", "Lityum İyon", null, null, 0 },
                    { new Guid("befd0719-3e64-0818-21d8-38708e58c5f7"), "JT Sprockets", "Ön/Arka Dişli", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, null, "JTF520.15", "15 Diş", "Ön Dişli", null, null, 0 },
                    { new Guid("cef44606-68a2-c1d2-2257-5d0d5cd3e805"), "DID", "Zincir", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Touring / Sport", "525VX3", "120 Bakla", "X-Ring", null, null, 0 },
                    { new Guid("d4f4a4ea-20a2-9d2a-2e8b-c86c3084ef39"), "K&N", "Hava Filtresi", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Yıkanabilir", "YA-9015", null, "Performans", null, null, 0 },
                    { new Guid("ddf7d89a-5e92-bdcb-282e-e2643200fd75"), "Castrol", "Soğutma Sıvısı", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Uzun ömürlü antifriz", "Radicool SF", "Organik", "Hazır Karışım", null, null, 0 },
                    { new Guid("fa136b44-cd6e-55de-db4d-5c504e71bba0"), "EBC", "Fren Balatası", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Yüksek performanslı frenleme", "FA252HH", null, "Sinterli (Ön)", null, null, 0 },
                    { new Guid("fdb49975-7c12-efcd-fac1-af79799f7962"), "Motul", "Motor Yağı", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Standart kullanım", "5100 10W-40 4T", "API SM, JASO MA2", "4T Yarı Sentetik", null, null, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableCatalogItems_TenantId_Category_Brand_ProductName",
                table: "ConsumableCatalogItems",
                columns: new[] { "TenantId", "Category", "Brand", "ProductName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableSuggestions_TenantId_Type_NormalizedValue",
                table: "ConsumableSuggestions",
                columns: new[] { "TenantId", "Type", "NormalizedValue" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumableCatalogItems");

            migrationBuilder.DropTable(
                name: "ConsumableSuggestions");
        }
    }
}
