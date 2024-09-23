using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddressandBuyerstablesedit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Countries_CountryCode",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CountryCode",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "No",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Buyers",
                newName: "BuyerCode");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Buyers",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            // below Fluent migration code commnted and edited manullay as cannot change
            // column of int to guid type by fluent migrator - Drop and add manually
            // https://stackoverflow.com/questions/36599296/entity-framework-change-key-type

            //migrationBuilder.AlterColumn<Guid>(
            //    name: "AddressId",
            //    table: "Buyers",
            //    type: "uniqueidentifier",
            //    nullable: true,
            //    oldClrType: typeof(int),
            //    oldType: "int");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Buyers");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "Buyers",
                type: "uniqueidentifier",
                nullable: true);

            // end of change

            migrationBuilder.AddColumn<string>(
                name: "CUSDEC",
                table: "Buyers",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Buyers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "Addresses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Addresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            // start change
            //migrationBuilder.AlterColumn<int>(
            //    name: "AddressType",
            //    table: "Addresses",
            //    type: "int",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            migrationBuilder.DropColumn(
                 name: "AddressType",
                 table: "Addresses");

            migrationBuilder.AddColumn<int>(
                   name: "AddressType",
                   table: "Addresses",
                   type: "int",
                   nullable: true);

            //migrationBuilder.AlterColumn<Guid>(
            //    name: "AddressId",
            //    table: "Addresses",
            //    type: "Guid",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int");

            migrationBuilder.DropColumn(
              name: "AddressId",
              table: "Addresses");

            migrationBuilder.AddColumn<Guid>(
              name: "AddressId",
              table: "Addresses",
              type: "uniqueidentifier",
              nullable: true);

            // end of change

            migrationBuilder.AddColumn<int>(
                name: "PostCode",
                table: "Addresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "Addresses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CUSDEC",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "PostCode",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "BuyerCode",
                table: "Buyers",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Buyers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "Buyers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "Addresses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Addresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressType",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "Addresses",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "Guid");

            migrationBuilder.AddColumn<string>(
                name: "No",
                table: "Addresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Addresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CountryCode",
                table: "Addresses",
                column: "CountryCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Countries_CountryCode",
                table: "Addresses",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
