using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBankCodeFromCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Currencies_Banks_CountryCode",
                table: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Currencies_CountryCode",
                table: "Currencies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CountryCode",
                table: "Currencies",
                column: "CountryCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Currencies_Banks_CountryCode",
                table: "Currencies",
                column: "CountryCode",
                principalTable: "Banks",
                principalColumn: "BankCode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
