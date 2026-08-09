using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sub Contracts (Order Management -> D. Sub Contracts, od_subc1.prg / od_subc1.dbf).
            // See SubContract.cs for the full class-level history/decision notes (2026-08-09).
            migrationBuilder.CreateTable(
                name: "SubContracts",
                columns: table => new
                {
                    Buyer = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Style = table.Column<string>(type: "varchar(20)", nullable: false),
                    SubContractorCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    SubQuantity = table.Column<decimal>(type: "decimal(9,0)", nullable: false),
                    CostPerGarment = table.Column<decimal>(type: "decimal(11,2)", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(9,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubContracts", x => new { x.Buyer, x.Order, x.Type, x.Style, x.SubContractorCode });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubContracts");
        }
    }
}
