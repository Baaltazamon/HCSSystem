using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCSSystem.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMeterToAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meters_Users_UserId",
                table: "Meters");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Meters",
                newName: "AddressId");

            migrationBuilder.RenameIndex(
                name: "IX_Meters_UserId",
                table: "Meters",
                newName: "IX_Meters_AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meters_Addresses_AddressId",
                table: "Meters",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meters_Addresses_AddressId",
                table: "Meters");

            migrationBuilder.RenameColumn(
                name: "AddressId",
                table: "Meters",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Meters_AddressId",
                table: "Meters",
                newName: "IX_Meters_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meters_Users_UserId",
                table: "Meters",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
