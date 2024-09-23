using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateaddressbuyerrelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.AlterColumn<Guid>(
                name: "AddressId",
                table: "Buyers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostCode",
                table: "Addresses",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "Addresses",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuyerCode",
                table: "Addresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Default",
                table: "Addresses",
                type: "bit",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Buyers_AddressId",
                table: "Buyers",
                column: "AddressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                columns: new[] { "Id", "AddressId" });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AddressId",
                table: "Addresses",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Buyers_AddressId",
                table: "Addresses",
                column: "AddressId",
                principalTable: "Buyers",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Buyers_AddressId",
                table: "Addresses");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Buyers_AddressId",
                table: "Buyers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_AddressId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "BuyerCode",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Default",
                table: "Addresses");

            migrationBuilder.AlterColumn<Guid>(
                name: "AddressId",
                table: "Buyers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostCode",
                table: "Addresses",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "Addresses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "Id");
        }
    }
}
