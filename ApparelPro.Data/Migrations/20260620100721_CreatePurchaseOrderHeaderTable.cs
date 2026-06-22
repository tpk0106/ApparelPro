using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatePurchaseOrderHeaderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseOrderHeaders",
                columns: table => new
                {
                    PurchaseOrderNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    SupplierCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    StoreCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    ProformaInvoiceNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    ProformaInvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    IsPoUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderHeaders", x => x.PurchaseOrderNumber);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderHeaders");
        }
    }
}
