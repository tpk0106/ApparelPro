using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class GinTraceabilityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BalanceToReceive",
                table: "OrderwiseStockTransactions",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocumentNumber",
                table: "OrderwiseStockTransactions",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "OrderwiseStockTransactions",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "OrderwiseStockTransactions",
                type: "varchar(3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IssuedQuantity",
                table: "OrderwiseStockMasters",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            // Idempotent seed: only insert the GIN sequence row if it doesn't already exist.
            // SharedService.GenerateNextDocumentNumberAsync throws if the NoteType row is missing,
            // so this row must exist before the first GIN can ever be raised.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM DocumentSequences WHERE NoteType = 'GIN')
                BEGIN
                    INSERT INTO DocumentSequences (NoteType, LastAllocatedNumber, Prefix)
                    VALUES ('GIN', 0, '')
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DocumentSequences WHERE NoteType = 'GIN'");

            migrationBuilder.DropColumn(
                name: "IssuedQuantity",
                table: "OrderwiseStockMasters");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "OrderwiseStockTransactions");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "OrderwiseStockTransactions");

            migrationBuilder.DropColumn(
                name: "SourceDocumentNumber",
                table: "OrderwiseStockTransactions");

            migrationBuilder.DropColumn(
                name: "BalanceToReceive",
                table: "OrderwiseStockTransactions");
        }
    }
}
