using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace MotorCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceOrderLinePricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "ServicePartItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ServicePartItems",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConsumablesTotal",
                table: "ServiceOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "ServiceOperationItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ServiceOperationItems",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ServiceOperationItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceCatalogItemId",
                table: "ServiceOperationItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "ServiceOperationItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ServiceConsumableItems",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "ServiceConsumableItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                "UPDATE \"ServiceOperationItems\" SET \"Quantity\" = 1, \"UnitPrice\" = \"Price\" WHERE \"UnitPrice\" = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "ServicePartItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ServicePartItems");

            migrationBuilder.DropColumn(
                name: "ConsumablesTotal",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "ServiceOperationItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ServiceOperationItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ServiceOperationItems");

            migrationBuilder.DropColumn(
                name: "ServiceCatalogItemId",
                table: "ServiceOperationItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ServiceOperationItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ServiceConsumableItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ServiceConsumableItems");
        }
    }
}
