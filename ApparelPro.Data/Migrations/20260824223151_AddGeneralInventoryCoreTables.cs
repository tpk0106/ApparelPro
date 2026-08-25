using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralInventoryCoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralPurchaseOrders",
                columns: table => new
                {
                    PoNumber = table.Column<string>(type: "varchar(6)", nullable: false),
                    SupplierCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OrderTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    BasisCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    ProformaInvoiceNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    ProformaInvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    UserId = table.Column<string>(type: "varchar(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralPurchaseOrders", x => x.PoNumber);
                });

            migrationBuilder.CreateTable(
                name: "GeneralStockMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(22)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    QtyInHand = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    ShadowBalance = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    Value = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    Currency = table.Column<string>(type: "varchar(3)", nullable: false),
                    DamagedQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    ReorderLevel = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    ReorderQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    MinStock = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    MaxStock = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralStockMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralStockMasters_Currencies_Currency",
                        column: x => x.Currency,
                        principalTable: "Currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneralStockMasters_Units_Unit",
                        column: x => x.Unit,
                        principalTable: "Units",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeneralStockTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionTypeCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    DocumentNumber = table.Column<string>(type: "varchar(6)", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TransactionTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    StoreCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(22)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    SupplierCode = table.Column<string>(type: "varchar(6)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(12,4)", nullable: false, defaultValue: 0m),
                    Currency = table.Column<string>(type: "varchar(3)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(7,3)", nullable: true),
                    BuyerCode = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<string>(type: "varchar(12)", nullable: true),
                    PoNumber = table.Column<string>(type: "varchar(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralStockTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneralPurchaseOrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoNumber = table.Column<string>(type: "varchar(6)", nullable: false),
                    StoreCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(22)", nullable: false),
                    RefNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    ExpectedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralPurchaseOrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralPurchaseOrderDetails_GeneralPurchaseOrders_PoNumber",
                        column: x => x.PoNumber,
                        principalTable: "GeneralPurchaseOrders",
                        principalColumn: "PoNumber",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralPurchaseOrderDetails_PoNumber",
                table: "GeneralPurchaseOrderDetails",
                column: "PoNumber");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralStockMasters_Currency",
                table: "GeneralStockMasters",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralStockMasters_StoreCode_ItemCode",
                table: "GeneralStockMasters",
                columns: new[] { "StoreCode", "ItemCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneralStockMasters_Unit",
                table: "GeneralStockMasters",
                column: "Unit");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralStockTransactions_StoreCode_ItemCode_TransactionDate",
                table: "GeneralStockTransactions",
                columns: new[] { "StoreCode", "ItemCode", "TransactionDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralPurchaseOrderDetails");

            migrationBuilder.DropTable(
                name: "GeneralStockMasters");

            migrationBuilder.DropTable(
                name: "GeneralStockTransactions");

            migrationBuilder.DropTable(
                name: "GeneralPurchaseOrders");
        }
    }
}
