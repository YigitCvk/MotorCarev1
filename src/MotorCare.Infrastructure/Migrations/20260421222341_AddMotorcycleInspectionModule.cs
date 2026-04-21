using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMotorcycleInspectionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MotorcycleInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InspectionNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Plate = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Mileage = table.Column<int>(type: "integer", nullable: true),
                    ChassisNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EngineNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Query5664 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MileageQuery = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PackageType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PackagePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GeneralNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TestRideNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CosmeticNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorcycleInspections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotorcycleInspectionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Result = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    MotorcycleInspectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorcycleInspectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotorcycleInspectionItems_MotorcycleInspections_MotorcycleI~",
                        column: x => x.MotorcycleInspectionId,
                        principalTable: "MotorcycleInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleInspectionItems_MotorcycleInspectionId",
                table: "MotorcycleInspectionItems",
                column: "MotorcycleInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleInspections_TenantId",
                table: "MotorcycleInspections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleInspections_TenantId_InspectionNo",
                table: "MotorcycleInspections",
                columns: new[] { "TenantId", "InspectionNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotorcycleInspectionItems");

            migrationBuilder.DropTable(
                name: "MotorcycleInspections");
        }
    }
}
