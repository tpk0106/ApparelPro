using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialInvoiceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommercialInvoiceHeaders",
                columns: table => new
                {
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "date", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    DocumentaryBuyerCode = table.Column<string>(type: "varchar(6)", nullable: true),
                    NotifyPartyCode = table.Column<string>(type: "varchar(6)", nullable: true),
                    ConsigneeCode = table.Column<string>(type: "varchar(6)", nullable: true),
                    LoadPortCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    DestinationCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    ContinuingDestinationCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    CarrierCode = table.Column<string>(type: "varchar(6)", nullable: true),
                    ShipDate = table.Column<DateTime>(type: "date", nullable: true),
                    LcNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    LcDate = table.Column<DateTime>(type: "date", nullable: true),
                    AssessmentNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    IssuingBankCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    Remark1 = table.Column<string>(type: "varchar(40)", nullable: true),
                    Remark2 = table.Column<string>(type: "varchar(40)", nullable: true),
                    Remark3 = table.Column<string>(type: "varchar(40)", nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    TradeTermCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    TradeTermLine1 = table.Column<string>(type: "varchar(30)", nullable: true),
                    TradeTermLine2 = table.Column<string>(type: "varchar(30)", nullable: true),
                    TradeTermLine3 = table.Column<string>(type: "varchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialInvoiceHeaders", x => x.InvoiceNumber);
                    table.ForeignKey(
                        name: "FK_CommercialInvoiceHeaders_Buyers_BuyerCode",
                        column: x => x.BuyerCode,
                        principalTable: "Buyers",
                        principalColumn: "BuyerCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommercialInvoiceHeaders_Destinations_DestinationCode",
                        column: x => x.DestinationCode,
                        principalTable: "Destinations",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommercialInvoiceLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "nvarchar(12)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "nvarchar(12)", nullable: false),
                    NewOrder = table.Column<string>(type: "varchar(12)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    QuotaCategory = table.Column<string>(type: "varchar(6)", nullable: false),
                    FromYearMonth = table.Column<string>(type: "varchar(5)", nullable: false),
                    ToYearMonth = table.Column<string>(type: "varchar(5)", nullable: false),
                    QuotaCountry = table.Column<string>(type: "varchar(10)", nullable: false),
                    PackingMedia = table.Column<string>(type: "varchar(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialInvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommercialInvoiceLines_CommercialInvoiceHeaders_InvoiceNumber",
                        column: x => x.InvoiceNumber,
                        principalTable: "CommercialInvoiceHeaders",
                        principalColumn: "InvoiceNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommercialInvoiceLines_Styles_BuyerCode_Order_TypeCode_StyleCode",
                        columns: x => new { x.BuyerCode, x.Order, x.TypeCode, x.StyleCode },
                        principalTable: "Styles",
                        principalColumns: new[] { "Buyer", "Order", "Type", "Style" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommercialInvoiceLines_Units_Unit",
                        column: x => x.Unit,
                        principalTable: "Units",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommercialInvoiceHeaders_BuyerCode",
                table: "CommercialInvoiceHeaders",
                column: "BuyerCode");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialInvoiceHeaders_DestinationCode",
                table: "CommercialInvoiceHeaders",
                column: "DestinationCode");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialInvoiceLines_BuyerCode_Order_TypeCode_StyleCode_NewOrder",
                table: "CommercialInvoiceLines",
                columns: new[] { "BuyerCode", "Order", "TypeCode", "StyleCode", "NewOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CommercialInvoiceLines_InvoiceNumber",
                table: "CommercialInvoiceLines",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialInvoiceLines_Unit",
                table: "CommercialInvoiceLines",
                column: "Unit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommercialInvoiceLines");

            migrationBuilder.DropTable(
                name: "CommercialInvoiceHeaders");
        }
    }
}
