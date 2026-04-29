using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MotorCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleEngineNumberAndExpandConsumables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumableSuggestions");

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("0e0d3a70-d057-28c9-a58a-4a9259794ce6"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("144dd497-8ee6-7fa7-71b0-3fc88bbbc0be"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("176f59e6-b1d6-bd62-7a9c-500c0e6113ca"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("1ad8db7b-e41a-5d7b-8d34-f33366c89154"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("25fce551-8ff4-39ee-9f08-0237bd1b726d"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("3657f151-ffdb-21a6-f4e9-6c8b7e2db3a0"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("4c23f151-df44-38da-99a1-3d88a49b81fe"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("5e047430-f336-a816-6e3a-8abf1c726e7b"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("602d9914-2dac-c3a9-6875-ae26126349eb"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("67ad2600-961e-1c0a-6fae-9bf7528f985e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("6f4a08c5-cedf-4d19-5ebb-8d6086f819cd"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("6f7949a6-c8dc-b7eb-cd93-f18e831126ac"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("70f0e37c-5a0c-c60d-ceeb-877faefc7cd2"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("715216fe-59bb-c2a9-c2ac-f81a4627b88e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("8a8cc886-0952-5bbf-6b02-2e0d54e2d5fd"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("8be6fb80-a059-fe30-fad6-630fa787bbd6"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("8fcbb815-85b1-b050-a792-7552c370deef"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("aa2ec53c-e352-75a8-8fee-3bcaa8db5a30"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("b15a7733-9b78-f202-18b5-746c667f2b1e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("b241b9ca-d2cc-ece5-8496-8265f8eb5ea8"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("b332bc91-58f1-fd39-858e-e51c9814bfa0"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("b5da6d70-39fe-b127-5a64-d3953000617b"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("b866391a-2aec-8160-6c45-6b97ab203665"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("befd0719-3e64-0818-21d8-38708e58c5f7"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("cef44606-68a2-c1d2-2257-5d0d5cd3e805"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("d4f4a4ea-20a2-9d2a-2e8b-c86c3084ef39"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("ddf7d89a-5e92-bdcb-282e-e2643200fd75"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("fa136b44-cd6e-55de-db4d-5c504e71bba0"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("fdb49975-7c12-efcd-fac1-af79799f7962"));

            migrationBuilder.AddColumn<string>(
                name: "EngineNumber",
                table: "Vehicles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubCategory",
                table: "ConsumableCatalogItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Specification",
                table: "ConsumableCatalogItems",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "ConsumableCatalogItems",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ConsumableCatalogItems",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "ConsumableCatalogItems",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.InsertData(
                table: "ConsumableCatalogItems",
                columns: new[] { "Id", "Brand", "Category", "CreatedAt", "IsActive", "IsSystemDefault", "Notes", "ProductName", "Specification", "SubCategory", "TenantId", "UpdatedAt", "UsageCount" },
                values: new object[,]
                {
                    { new Guid("00363461-c24b-7560-2dab-7c0d9c1f118c"), "Hiflofiltro", "Yağ Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "HF204", "Örnek referans", "Spin-on / kartuş", null, null, 0 },
                    { new Guid("06024250-45c8-444f-2e15-7d845275336e"), "Yuasa", "Akü", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Sport / naked / scooter uygulamaları", "YTZ10S", "12V 9.1Ah 190 CCA", "AGM / MF", null, null, 0 },
                    { new Guid("0b6ad96e-757d-ea21-7550-f30dd82dbf2e"), "RK", "Zincir", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Big-cc / road race / superbike", "530 ZXW / GXW serisi", "XW-Ring", "XW-Ring", null, null, 0 },
                    { new Guid("0e7d2dc1-6099-390b-5f6e-a01c85f4de11"), "Hiflofiltro", "Hava Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Örn. XMAX 250/300 sınıfı uygulamalar", "HFA4301", "Örnek referans", "OEM replacement", null, null, 0 },
                    { new Guid("138d43e5-0de8-f1bd-91ec-fd71256388fb"), "Hiflofiltro", "Hava Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Örn. Forza 250 sınıfı uygulamalar", "HFA1304", "Örnek referans", "OEM replacement", null, null, 0 },
                    { new Guid("13b111c6-b411-d084-ac4d-fb0b0e86f8bb"), "K&N", "Yağ Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "KN-204", "Örnek referans", "Performance", null, null, 0 },
                    { new Guid("1aefae80-029a-272f-8f9f-dbeb062d3dc6"), "Denso", "Buji", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "X24ESR-U", "Örnek referans", "Standart nikel", null, null, 0 },
                    { new Guid("1b7a9a5e-b15f-32f8-a647-206ece844530"), "K&N", "Hava Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "KTM uygulamaları dahil performans tipi", "KT-1113", "Örnek referans", "Yıkanabilir / performance", null, null, 0 },
                    { new Guid("25b6e6de-d73a-d1ec-a68c-38636424831b"), "Motorex", "Soğutma Sıvısı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Alüminyum motorlarda yaygın premium seçenek", "Coolant M3.0", "1L / OAT ready to use", "Hazır kullanım", null, null, 0 },
                    { new Guid("3d8d0387-4617-2cc4-1aae-50f0e67649b2"), "EBC", "Fren Balatası", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Scooter / street arka uygulamalar", "SFA197", "Örnek referans", "Organik", null, null, 0 },
                    { new Guid("42142bf6-ef47-3974-e9b9-f4a05fdf070e"), "Motul", "Fren Hidroliği", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Standart bakım / fren & debriyaj hidrolik sistemleri", "DOT 3&4 Brake Fluid", "0.5L", "DOT 3/4", null, null, 0 },
                    { new Guid("44c385c7-ab87-e673-a965-5efb37ceadb7"), "BS Battery", "Akü", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "125-300cc sınıfında yaygın alternatif", "BTX7L-BS", "12V 6.3Ah 100A EN", "SLA / MF", null, null, 0 },
                    { new Guid("5071ae2e-d029-ca84-a8a9-3b3cbf6bcd92"), "NGK", "Buji", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Daha uzun ömür / performans", "CR9EIX", "Örnek referans", "İridyum", null, null, 0 },
                    { new Guid("52a69454-0be6-b474-a4da-d0a606dd9831"), "Liqui Moly", "Motor Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Ekonomik bakım paketi", "10W40 4T Basic Street", "Basic Street", "4T Mineral / Ekonomik", null, null, 0 },
                    { new Guid("5efc14bb-8123-9ff2-6427-456e3c12c8d2"), "K&N", "Yağ Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Harley vb. bazı modeller", "KN-171B", "Örnek referans", "Performance", null, null, 0 },
                    { new Guid("63af54b0-50bc-9768-e595-bf6a12df6e63"), "DID", "Zincir", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Orta-üst segment street / touring", "525VX3", "525 pitch / VX3", "X-Ring", null, null, 0 },
                    { new Guid("6f04210e-7035-54e0-a9ac-422fe123b851"), "SBS", "Fren Balatası", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Scooter / street arka uygulamalar", "230HM", "Örnek referans", "Seramik / street", null, null, 0 },
                    { new Guid("6f76b8e8-87eb-002e-cb96-b1de531c22b4"), "Motul", "Fork Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Ön maşa bakımında", "Fork Oil Expert 5W", "5W / 1L", "Suspension oil", null, null, 0 },
                    { new Guid("704e5406-b386-6fa1-4cbc-95511ecdae99"), "Motul", "Fren Hidroliği", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "ABS/ESP uyumlu sistemler dahil", "DOT 4 LV", "Class 6 / düşük viskozite", "DOT 4 LV", null, null, 0 },
                    { new Guid("74df8308-8b94-16ba-69a7-df89876afdf0"), "Castrol", "Motor Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Street / commuter / scooter", "POWER1 4T 10W-40", "POWER1 serisi", "4T Sentetik", null, null, 0 },
                    { new Guid("78ab1c51-b99f-6345-f9c1-fc5ef7be5588"), "BS Battery", "Akü", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Orta segment street / touring", "BTZ10S", "12V 9Ah 190A EN", "SLA / MF", null, null, 0 },
                    { new Guid("7daefe13-2181-ec14-3ef9-dc2407ab4fce"), "JT Sprockets", "Arka Dişli", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "JTR300.46", "Örnek referans", "Çelik", null, null, 0 },
                    { new Guid("83515191-0df2-cf7f-12a3-d52f7b910981"), "Liqui Moly", "Motor Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Street / commuter", "Motorbike 4T 10W-40 Street", "10W-40 Street", "4T Sentetik Teknoloji", null, null, 0 },
                    { new Guid("86707dd5-c92e-6157-f5b3-7b2e7119ccc3"), "Motul", "Fork Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Ön maşa bakımında", "Fork Oil Expert 10W", "10W / 1L", "Suspension oil", null, null, 0 },
                    { new Guid("875bb55c-98fb-809d-768c-3be2795d7227"), "Motul", "Motor Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Günlük kullanım / performans / street", "7100 10W-40 4T", "API SP, JASO MA2", "4T Tam Sentetik", null, null, 0 },
                    { new Guid("8f8ef9dd-855b-cfcb-7382-80f6144f7884"), "K&N", "Yağ Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "KN-164", "Örnek referans", "Performance", null, null, 0 },
                    { new Guid("967af2a5-0233-d780-f8b3-e9f22240ed0e"), "Motul", "Soğutma Sıvısı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Sıvı soğutmalı motosikletler", "Motocool Factory Line -35C", "1L / Organic+", "Hazır kullanım", null, null, 0 },
                    { new Guid("96f87973-47d5-0915-1c04-ec1de3ec595a"), "NGK", "Buji", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Scooter / street uygulamaları", "CPR8EA-9", "Örnek referans", "Standart nikel", null, null, 0 },
                    { new Guid("9f03da2b-6355-2621-09e8-88c2be019f9f"), "Yuasa", "Akü", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Orta-üst segment", "YTX14-BS", "12V 12.6Ah", "AGM / MF", null, null, 0 },
                    { new Guid("9f2240b7-5e6d-c3a4-b133-dc73e996b561"), "NG Brakes", "Fren Diski", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Ön / arka disk", "Genel seri", "Modele göre referans değişir", "Round / fixed / floating", null, null, 0 },
                    { new Guid("ab3a82f9-daee-30d6-6ef5-c41d0ff64bc7"), "K&N", "Hava Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "BMW uygulamaları dahil performance tipi", "BM-1204", "Örnek referans", "Yıkanabilir / performance", null, null, 0 },
                    { new Guid("af7d8707-50ef-b009-375d-4f1b0928a32e"), "JT Sprockets", "Ön Dişli", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "JTF1586.16RB", "Örnek referans", "Çelik", null, null, 0 },
                    { new Guid("b5eb00e0-e22e-11ab-3ab9-88d6f3b95074"), "DID", "Zincir", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Street + off-road upgrade", "520VX3", "520 pitch / VX3", "X-Ring", null, null, 0 },
                    { new Guid("bb02d709-eff8-29c4-b0c0-c91e04b9a141"), "IPONE", "Motor Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Roadster / enduro / sportif GT", "Full Power Katana 10W-40", "PAO + ester / 10W-40", "4T Tam Sentetik", null, null, 0 },
                    { new Guid("c041a07e-26f8-7f39-c132-4fdd54c71356"), "Yuasa", "Akü", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Scooter / 125-300cc sınıfında sık görülür", "YTX7L-BS", "12V 6Ah 100 CCA", "AGM / MF", null, null, 0 },
                    { new Guid("c8ea833f-46de-d68c-1962-4aad66c5f83b"), "SBS", "Fren Balatası", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Scooter / street arka uygulamalar", "155HM", "Örnek referans", "Seramik / street", null, null, 0 },
                    { new Guid("ca4c7dce-4467-13fa-fd01-b718f77d21a2"), "Liqui Moly", "Fren Hidroliği", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "ABS'li sistemlerde tercih edilebilir", "Brake Fluid SL6 DOT 4 (21167)", "500 ml", "DOT 4 düşük viskozite", null, null, 0 },
                    { new Guid("d6fca400-e109-f36e-e8e4-52ec56879daa"), "Hiflofiltro", "Yağ Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "HF303", "Örnek referans", "Spin-on / kartuş", null, null, 0 },
                    { new Guid("d7147fdd-50d5-0a3e-1274-10c62eb585a4"), "Liqui Moly", "Fren Hidroliği", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Standart bakım", "Brake Fluid DOT 4 (3093)", "500 ml", "DOT 4", null, null, 0 },
                    { new Guid("dd6d594b-4522-b458-fa8b-52500faae932"), "DID + JT", "Zincir + Dişli Set", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "125cc commuter / sport commuter", "Did 428D + JT set", "Örnek kombinasyon", "Komple kit", null, null, 0 },
                    { new Guid("de78a5b9-93b2-5be8-45a0-15e8f31bcb7a"), "Hiflofiltro", "Yağ Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "HF132", "Örnek referans", "Spin-on / kartuş", null, null, 0 },
                    { new Guid("e190af3e-bfaf-40c0-82be-7a5af3575d9e"), "Shell", "Motor Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Modern 4T motosikletler", "Advance Ultra 4T", "Shell Advance Ultra", "4T Tam Sentetik", null, null, 0 },
                    { new Guid("e25c0545-37aa-10aa-8411-d30a8c56f58e"), "Yuasa", "Akü", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Orta segment street / touring", "YTX12-BS", "12V 10Ah 180 CCA", "AGM / MF", null, null, 0 },
                    { new Guid("e5dece29-227d-a4cb-c172-ab868e91e321"), "Motul", "Fren Hidroliği", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Yüksek sıcaklık / performans", "RBF 660 Factory Line", "0.5L", "Racing", null, null, 0 },
                    { new Guid("ecb44e8e-9693-6500-5016-b715bde61bb9"), "NGK", "Buji", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Cub / commuter / scooter sınıfında yaygın", "CR7HSA", "Örnek referans", "Standart nikel", null, null, 0 },
                    { new Guid("efd67b80-2d10-9402-7dae-de8cf9a4b1bc"), "Hiflofiltro", "Hava Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Örn. KTM 390 Duke sınıfı uygulamalar", "HFA6303", "Örnek referans", "OEM replacement", null, null, 0 },
                    { new Guid("f25c6ad6-1d46-3b63-b060-f6795a2e04e1"), "Brembo", "Fren Balatası", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "All-road / ön balata tarafında yaygın", "SA Pads", "SA bileşimi", "Sinter", null, null, 0 },
                    { new Guid("f26e49dc-1143-2c40-7f4c-7e14e925a702"), "Hiflofiltro", "Hava Filtresi", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "HFA1134", "Örnek referans", "OEM replacement", null, null, 0 },
                    { new Guid("f2cf18df-2726-488f-2748-d6455fde56e7"), "Brembo", "Fren Diski", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Premium replacement", "Serie Oro", "Fixed / floating seçenekleri", "Serie Oro", null, null, 0 },
                    { new Guid("f403e4f6-11c2-9782-95cd-c920a19ca889"), "EBC", "Fren Balatası", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "FA197", "Örnek referans", "Sinter / ön-arka varyantlı", null, null, 0 },
                    { new Guid("f4a3c123-7858-f34d-c49b-d387e8173f12"), "Motorex", "Fork Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Ön maşa bakımında premium seçenek", "Racing Fork Oil 5W", "5W / 1L", "Suspension oil", null, null, 0 },
                    { new Guid("f5df91d1-93fe-447f-dae4-5d96848c4c52"), "Motul", "Fork Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Ağır sönümleme isteyen uygulamalar", "Fork Oil Expert 20W", "20W / 1L", "Suspension oil", null, null, 0 },
                    { new Guid("f7ff63a3-1525-cc8f-c375-b3fc9d4c1afc"), "Motul", "Motor Yağı", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Düşük viskozite isteyen modeller", "7100 10W-30 4T", "API SP, JASO MA2", "4T Tam Sentetik", null, null, 0 },
                    { new Guid("fdf3bd4d-0590-8159-2af3-38740a6ccbf3"), "Denso", "Buji", new DateTimeOffset(new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, "Model bazlı değişir", "U24ESR-N", "Örnek referans", "Standart nikel", null, null, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("00363461-c24b-7560-2dab-7c0d9c1f118c"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("06024250-45c8-444f-2e15-7d845275336e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("0b6ad96e-757d-ea21-7550-f30dd82dbf2e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("0e7d2dc1-6099-390b-5f6e-a01c85f4de11"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("138d43e5-0de8-f1bd-91ec-fd71256388fb"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("13b111c6-b411-d084-ac4d-fb0b0e86f8bb"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("1aefae80-029a-272f-8f9f-dbeb062d3dc6"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("1b7a9a5e-b15f-32f8-a647-206ece844530"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("25b6e6de-d73a-d1ec-a68c-38636424831b"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("3d8d0387-4617-2cc4-1aae-50f0e67649b2"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("42142bf6-ef47-3974-e9b9-f4a05fdf070e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("44c385c7-ab87-e673-a965-5efb37ceadb7"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("5071ae2e-d029-ca84-a8a9-3b3cbf6bcd92"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("52a69454-0be6-b474-a4da-d0a606dd9831"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("5efc14bb-8123-9ff2-6427-456e3c12c8d2"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("63af54b0-50bc-9768-e595-bf6a12df6e63"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("6f04210e-7035-54e0-a9ac-422fe123b851"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("6f76b8e8-87eb-002e-cb96-b1de531c22b4"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("704e5406-b386-6fa1-4cbc-95511ecdae99"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("74df8308-8b94-16ba-69a7-df89876afdf0"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("78ab1c51-b99f-6345-f9c1-fc5ef7be5588"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("7daefe13-2181-ec14-3ef9-dc2407ab4fce"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("83515191-0df2-cf7f-12a3-d52f7b910981"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("86707dd5-c92e-6157-f5b3-7b2e7119ccc3"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("875bb55c-98fb-809d-768c-3be2795d7227"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("8f8ef9dd-855b-cfcb-7382-80f6144f7884"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("967af2a5-0233-d780-f8b3-e9f22240ed0e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("96f87973-47d5-0915-1c04-ec1de3ec595a"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("9f03da2b-6355-2621-09e8-88c2be019f9f"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("9f2240b7-5e6d-c3a4-b133-dc73e996b561"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("ab3a82f9-daee-30d6-6ef5-c41d0ff64bc7"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("af7d8707-50ef-b009-375d-4f1b0928a32e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("b5eb00e0-e22e-11ab-3ab9-88d6f3b95074"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("bb02d709-eff8-29c4-b0c0-c91e04b9a141"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("c041a07e-26f8-7f39-c132-4fdd54c71356"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("c8ea833f-46de-d68c-1962-4aad66c5f83b"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("ca4c7dce-4467-13fa-fd01-b718f77d21a2"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("d6fca400-e109-f36e-e8e4-52ec56879daa"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("d7147fdd-50d5-0a3e-1274-10c62eb585a4"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("dd6d594b-4522-b458-fa8b-52500faae932"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("de78a5b9-93b2-5be8-45a0-15e8f31bcb7a"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("e190af3e-bfaf-40c0-82be-7a5af3575d9e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("e25c0545-37aa-10aa-8411-d30a8c56f58e"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("e5dece29-227d-a4cb-c172-ab868e91e321"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("ecb44e8e-9693-6500-5016-b715bde61bb9"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("efd67b80-2d10-9402-7dae-de8cf9a4b1bc"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("f25c6ad6-1d46-3b63-b060-f6795a2e04e1"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("f26e49dc-1143-2c40-7f4c-7e14e925a702"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("f2cf18df-2726-488f-2748-d6455fde56e7"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("f403e4f6-11c2-9782-95cd-c920a19ca889"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("f4a3c123-7858-f34d-c49b-d387e8173f12"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("f5df91d1-93fe-447f-dae4-5d96848c4c52"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("f7ff63a3-1525-cc8f-c375-b3fc9d4c1afc"));

            migrationBuilder.DeleteData(
                table: "ConsumableCatalogItems",
                keyColumn: "Id",
                keyValue: new Guid("fdf3bd4d-0590-8159-2af3-38740a6ccbf3"));

            migrationBuilder.DropColumn(
                name: "EngineNumber",
                table: "Vehicles");

            migrationBuilder.AlterColumn<string>(
                name: "SubCategory",
                table: "ConsumableCatalogItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Specification",
                table: "ConsumableCatalogItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "ConsumableCatalogItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ConsumableCatalogItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "ConsumableCatalogItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80);

            migrationBuilder.CreateTable(
                name: "ConsumableSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
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
                name: "IX_ConsumableSuggestions_TenantId_Type_NormalizedValue",
                table: "ConsumableSuggestions",
                columns: new[] { "TenantId", "Type", "NormalizedValue" },
                unique: true);
        }
    }
}
