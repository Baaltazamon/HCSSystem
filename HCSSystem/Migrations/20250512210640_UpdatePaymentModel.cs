using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCSSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Clients_ClientId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Services_ServiceId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ClientId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Payments",
                newName: "MeterReadingId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_ServiceId",
                table: "Payments",
                newName: "IX_Payments_MeterReadingId");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_MeterReadings_MeterReadingId",
                table: "Payments",
                column: "MeterReadingId",
                principalTable: "MeterReadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_MeterReadings_MeterReadingId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "MeterReadingId",
                table: "Payments",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_MeterReadingId",
                table: "Payments",
                newName: "IX_Payments_ServiceId");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Payments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClientId",
                table: "Payments",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Clients_ClientId",
                table: "Payments",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Services_ServiceId",
                table: "Payments",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
