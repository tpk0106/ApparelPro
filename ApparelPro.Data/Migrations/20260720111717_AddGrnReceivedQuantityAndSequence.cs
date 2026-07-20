using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrnReceivedQuantityAndSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedQuantity",
                table: "OrderwiseStockMasters",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM DocumentSequences WHERE NoteType = 'GRN')
            BEGIN
                INSERT INTO DocumentSequences (NoteType, LastAllocatedNumber, Prefix)
                VALUES ('GRN', 0, '')
            END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DocumentSequences WHERE NoteType = 'GRN'");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "OrderwiseStockMasters");
        }
    }
}
