using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Tenants",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Website",
                table: "Tenants");
        }
    }
}
