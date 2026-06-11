using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleAddressesToBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Banks");

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "Addresses",
                type: "nvarchar(3)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BankCode",
                table: "Addresses",
                column: "BankCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Banks_BankCode",
                table: "Addresses",
                column: "BankCode",
                principalTable: "Banks",
                principalColumn: "BankCode",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Banks_BankCode",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_BankCode",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "Addresses");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Banks",
                type: "int",
                nullable: true);
        }
    }
}
