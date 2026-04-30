using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorCare.Infrastructure.Migrations
{
    [DbContext(typeof(Persistence.ApplicationDbContext))]
    [Migration("20260430003000_AddMotorcycleModelCatalogItems")]
    public partial class AddMotorcycleModelCatalogItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MotorcycleModelCatalogItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    BrandNormalized = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ModelNormalized = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ModelFamily = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Segment = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    EngineCc = table.Column<int>(type: "integer", nullable: true),
                    OriginCountry = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    OriginRegion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorcycleModelCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleModelCatalogItems_Brand",
                table: "MotorcycleModelCatalogItems",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleModelCatalogItems_Brand_Model",
                table: "MotorcycleModelCatalogItems",
                columns: new[] { "Brand", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleModelCatalogItems_BrandNormalized_ModelNormalized",
                table: "MotorcycleModelCatalogItems",
                columns: new[] { "BrandNormalized", "ModelNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleModelCatalogItems_IsActive",
                table: "MotorcycleModelCatalogItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleModelCatalogItems_OriginCountry",
                table: "MotorcycleModelCatalogItems",
                column: "OriginCountry");

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleModelCatalogItems_Segment",
                table: "MotorcycleModelCatalogItems",
                column: "Segment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotorcycleModelCatalogItems");
        }
    }
}
