using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenSaasAuthTokenIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSecurityTokens_TokenHash",
                table: "UserSecurityTokens");

            migrationBuilder.CreateIndex(
                name: "IX_UserSecurityTokens_TokenHash_Purpose",
                table: "UserSecurityTokens",
                columns: new[] { "TokenHash", "Purpose" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSecurityTokens_TokenHash_Purpose",
                table: "UserSecurityTokens");

            migrationBuilder.CreateIndex(
                name: "IX_UserSecurityTokens_TokenHash",
                table: "UserSecurityTokens",
                column: "TokenHash",
                unique: true);
        }
    }
}
