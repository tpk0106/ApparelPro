using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuotaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotaTransactions");

            migrationBuilder.DropColumn(
                name: "FromYearMonth",
                table: "PartShipments");

            migrationBuilder.DropColumn(
                name: "QuotaCategory",
                table: "PartShipments");

            migrationBuilder.DropColumn(
                name: "QuotaCountry",
                table: "PartShipments");

            migrationBuilder.DropColumn(
                name: "QuotaStatus",
                table: "PartShipments");

            migrationBuilder.DropColumn(
                name: "QuotaType",
                table: "PartShipments");

            migrationBuilder.DropColumn(
                name: "ToYearMonth",
                table: "PartShipments");

            migrationBuilder.DropColumn(
                name: "FromYearMonth",
                table: "CommercialInvoiceLines");

            migrationBuilder.DropColumn(
                name: "QuotaCategory",
                table: "CommercialInvoiceLines");

            migrationBuilder.DropColumn(
                name: "QuotaCountry",
                table: "CommercialInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ToYearMonth",
                table: "CommercialInvoiceLines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromYearMonth",
                table: "PartShipments",
                type: "varchar(5)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuotaCategory",
                table: "PartShipments",
                type: "varchar(6)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuotaCountry",
                table: "PartShipments",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuotaStatus",
                table: "PartShipments",
                type: "varchar(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuotaType",
                table: "PartShipments",
                type: "varchar(2)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToYearMonth",
                table: "PartShipments",
                type: "varchar(5)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FromYearMonth",
                table: "CommercialInvoiceLines",
                type: "varchar(5)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuotaCategory",
                table: "CommercialInvoiceLines",
                type: "varchar(6)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuotaCountry",
                table: "CommercialInvoiceLines",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToYearMonth",
                table: "CommercialInvoiceLines",
                type: "varchar(5)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "QuotaTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "varchar(3)", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    FromYearMonth = table.Column<string>(type: "varchar(5)", nullable: false),
                    NewOrder = table.Column<string>(type: "varchar(12)", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    QuotaCategory = table.Column<string>(type: "varchar(20)", nullable: false),
                    QuotaCountry = table.Column<string>(type: "varchar(10)", nullable: false),
                    QuotaStatus = table.Column<string>(type: "varchar(1)", nullable: false),
                    QuotaType = table.Column<string>(type: "varchar(2)", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(12)", nullable: false),
                    ToYearMonth = table.Column<string>(type: "varchar(5)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotaTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotaTransactions_Units_Unit",
                        column: x => x.Unit,
                        principalTable: "Units",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotaTransactions_Unit",
                table: "QuotaTransactions",
                column: "Unit");
        }
    }
}
