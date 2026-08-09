using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalIssueNoteSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sub Contractor reference (od_scref) - built as a prerequisite for AIN, see
            // SubContractor.cs for the full gap history.
            migrationBuilder.CreateTable(
                name: "SubContractors",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubContractors", x => x.Code);
                });

            // AIN traceability additions on OrderwiseStockTransactions - Sub-Contractor code
            // and Additional Process code for a '4X' row (legacy t_buyer/t_order, AIN's own
            // meaning - distinct from GTN's CounterpartyBuyerCode/CounterpartyOrder use of
            // the same two legacy columns).
            migrationBuilder.AddColumn<string>(
                name: "SubContractorCode",
                table: "OrderwiseStockTransactions",
                type: "varchar(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalProcessCode",
                table: "OrderwiseStockTransactions",
                type: "varchar(3)",
                nullable: true);

            // AIN's own dedicated running-total column on OrderwiseStockMasters - kept
            // separate from IssuedQuantity per explicit product decision (2026-08-09), see
            // OrderwiseStockMaster.AdditionalIssuedQuantity's own comments.
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalIssuedQuantity",
                table: "OrderwiseStockMasters",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            // Idempotent seed: only insert the AIN sequence row if it doesn't already exist.
            // SharedService.GenerateNextDocumentNumberAsync throws if the NoteType row is
            // missing, so this row must exist before the first AIN can ever be raised.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM DocumentSequences WHERE NoteType = 'AIN')
                BEGIN
                    INSERT INTO DocumentSequences (NoteType, LastAllocatedNumber, Prefix)
                    VALUES ('AIN', 0, '')
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DocumentSequences WHERE NoteType = 'AIN'");

            migrationBuilder.DropColumn(
                name: "AdditionalIssuedQuantity",
                table: "OrderwiseStockMasters");

            migrationBuilder.DropColumn(
                name: "AdditionalProcessCode",
                table: "OrderwiseStockTransactions");

            migrationBuilder.DropColumn(
                name: "SubContractorCode",
                table: "OrderwiseStockTransactions");

            migrationBuilder.DropTable(
                name: "SubContractors");
        }
    }
}
