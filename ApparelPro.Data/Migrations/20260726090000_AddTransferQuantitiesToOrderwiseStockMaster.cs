﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferQuantitiesToOrderwiseStockMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TransferInQuantity",
                table: "OrderwiseStockMasters",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransferOutQuantity",
                table: "OrderwiseStockMasters",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferInQuantity",
                table: "OrderwiseStockMasters");

            migrationBuilder.DropColumn(
                name: "TransferOutQuantity",
                table: "OrderwiseStockMasters");
        }
    }
}
