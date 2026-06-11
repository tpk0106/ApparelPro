using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleAddressesToBuyer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Buyers");

            migrationBuilder.AlterColumn<string>(
                name: "SwiftCode",
                table: "Banks",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuyerCode",
                table: "Addresses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BuyerCode",
                table: "Addresses",
                column: "BuyerCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Buyers_BuyerCode",
                table: "Addresses",
                column: "BuyerCode",
                principalTable: "Buyers",
                principalColumn: "BuyerCode",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Buyers_BuyerCode",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_BuyerCode",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "BuyerCode",
                table: "Addresses");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "Buyers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SwiftCode",
                table: "Banks",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11);
        }
    }
}
