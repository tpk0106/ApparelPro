using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropCommercialInvoiceHeaderDestinationFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommercialInvoiceHeaders_Destinations_DestinationCode",
                table: "CommercialInvoiceHeaders");

            migrationBuilder.DropIndex(
                name: "IX_CommercialInvoiceHeaders_DestinationCode",
                table: "CommercialInvoiceHeaders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CommercialInvoiceHeaders_DestinationCode",
                table: "CommercialInvoiceHeaders",
                column: "DestinationCode");

            migrationBuilder.AddForeignKey(
                name: "FK_CommercialInvoiceHeaders_Destinations_DestinationCode",
                table: "CommercialInvoiceHeaders",
                column: "DestinationCode",
                principalTable: "Destinations",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
