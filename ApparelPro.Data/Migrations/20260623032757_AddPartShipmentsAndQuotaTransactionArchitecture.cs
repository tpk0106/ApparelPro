using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartShipmentsAndQuotaTransactionArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartShipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(12)", nullable: false),
                    NewOrder = table.Column<string>(type: "varchar(12)", nullable: false),
                    DestinationCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    ShipDate = table.Column<DateTime>(type: "date", nullable: false),
                    SubContractFlag = table.Column<string>(type: "varchar(1)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ShippingMode = table.Column<string>(type: "varchar(3)", nullable: false),
                    QuotaCountry = table.Column<string>(type: "varchar(10)", nullable: false),
                    QuotaStatus = table.Column<string>(type: "varchar(1)", nullable: false),
                    QuotaCategory = table.Column<string>(type: "varchar(6)", nullable: false),
                    QuotaType = table.Column<string>(type: "varchar(2)", nullable: false),
                    FromYearMonth = table.Column<string>(type: "varchar(5)", nullable: false),
                    ToYearMonth = table.Column<string>(type: "varchar(5)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "date", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartShipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuotaTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "varchar(3)", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(12)", nullable: false),
                    NewOrder = table.Column<string>(type: "varchar(12)", nullable: false),
                    QuotaStatus = table.Column<string>(type: "varchar(1)", nullable: false),
                    FromYearMonth = table.Column<string>(type: "varchar(5)", nullable: false),
                    ToYearMonth = table.Column<string>(type: "varchar(5)", nullable: false),
                    QuotaCountry = table.Column<string>(type: "varchar(10)", nullable: false),
                    QuotaCategory = table.Column<string>(type: "varchar(20)", nullable: false),
                    QuotaType = table.Column<string>(type: "varchar(2)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotaTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartShipments_BuyerCode_Order_TypeCode_StyleCode_NewOrder_DestinationCode_ShipDate",
                table: "PartShipments",
                columns: new[] { "BuyerCode", "Order", "TypeCode", "StyleCode", "NewOrder", "DestinationCode", "ShipDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartShipments");

            migrationBuilder.DropTable(
                name: "QuotaTransactions");
        }
    }
}
