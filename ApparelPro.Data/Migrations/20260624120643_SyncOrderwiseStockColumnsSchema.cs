using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncOrderwiseStockColumnsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DamagedQuantity",
                table: "OrderwiseStocks",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDateIssued",
                table: "OrderwiseStocks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDateReceived",
                table: "OrderwiseStocks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyInHand",
                table: "OrderwiseStocks",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShadowBalance",
                table: "OrderwiseStocks",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SrnBalance",
                table: "OrderwiseStocks",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToDateIssued",
                table: "OrderwiseStocks",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToDateReceived",
                table: "OrderwiseStocks",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamagedQuantity",
                table: "OrderwiseStocks");

            migrationBuilder.DropColumn(
                name: "LastDateIssued",
                table: "OrderwiseStocks");

            migrationBuilder.DropColumn(
                name: "LastDateReceived",
                table: "OrderwiseStocks");

            migrationBuilder.DropColumn(
                name: "QtyInHand",
                table: "OrderwiseStocks");

            migrationBuilder.DropColumn(
                name: "ShadowBalance",
                table: "OrderwiseStocks");

            migrationBuilder.DropColumn(
                name: "SrnBalance",
                table: "OrderwiseStocks");

            migrationBuilder.DropColumn(
                name: "ToDateIssued",
                table: "OrderwiseStocks");

            migrationBuilder.DropColumn(
                name: "ToDateReceived",
                table: "OrderwiseStocks");
        }
    }
}
