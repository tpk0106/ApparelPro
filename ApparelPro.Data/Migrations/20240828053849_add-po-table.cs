using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class addpotable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    GarmentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    UnitCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Season = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BasisCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BasisValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => new { x.BuyerCode, x.Order });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrders");
        }
    }
}
