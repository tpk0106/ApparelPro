using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemParametersAndPurchaseOrderAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "QuantityOverriddenAt",
                table: "PurchaseOrders",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuantityOverriddenBy",
                table: "PurchaseOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    ParameterKey = table.Column<string>(type: "varchar(50)", nullable: false),
                    Value = table.Column<string>(type: "varchar(200)", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.ParameterKey);
                });

            migrationBuilder.InsertData(
                table: "SystemParameters",
                columns: new[] { "ParameterKey", "Description", "Value" },
                values: new object[] { "AllowOrderQuantityOverride", "When false (default), the sum of a purchase order's style quantities (converted into the order's own unit) cannot exceed the order's Total Quantity - Add/Update Style Details is rejected if it would. When true, the save is allowed even if it exceeds Total Quantity (the Styles grid still visually flags the overage).", "false" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "QuantityOverriddenAt",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "QuantityOverriddenBy",
                table: "PurchaseOrders");
        }
    }
}
