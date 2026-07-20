using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameSrnBalanceToStrnBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Naming consistency fix: the live/shipped convention for this feature is "STRN"
            // (DocumentSequence key "STRN", STRNController, STRNServiceModel), but this column
            // was left over from before that rename and still used the bare "SRN" abbreviation,
            // which risks colliding with "Supplier Return Note" in standard apparel-ERP terminology.
            migrationBuilder.RenameColumn(
                name: "SrnBalance",
                table: "OrderwiseStocks",
                newName: "StrnBalance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StrnBalance",
                table: "OrderwiseStocks",
                newName: "SrnBalance");
        }
    }
}
