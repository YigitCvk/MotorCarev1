using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryItemIdToServicePartItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryItemId",
                table: "ServicePartItems",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InventoryItemId",
                table: "ServicePartItems");
        }
    }
}
