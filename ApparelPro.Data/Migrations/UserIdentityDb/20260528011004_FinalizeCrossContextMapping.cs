using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations.UserIdentityDb
{
    /// <inheritdoc />
    public partial class FinalizeCrossContextMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // COMMENTED OUT: These are already dropped or don't exist anymore on your disk
            // migrationBuilder.DropUniqueConstraint(
            //     name: "AK_Address_AddressId",
            //     table: "Address");

            // migrationBuilder.DropPrimaryKey(
            //     name: "PK_Address",
            //     table: "Address");

            // migrationBuilder.RenameTable(
            //     name: "Address",
            //     newName: "Addresses");

            // KEEP THIS COMMENTED: This constraint already exists thanks to ApparelProDbContext
            // migrationBuilder.AddUniqueConstraint(
            //     name: "AK_Addresses_AddressId",
            //     table: "Addresses",
            //     column: "AddressId");

            // KEEP THIS COMMENTED: This primary key already exists thanks to ApparelProDbContext
            // migrationBuilder.AddPrimaryKey(
            //     name: "PK_Addresses",
            //     table: "Addresses",
            //     column: "Id");

            // CRITICAL KEEP: This is the single piece of code that adds your new safe constraint!

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Addresses_AddressId",
                table: "AspNetUsers",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Addresses_AddressId",
                table: "AspNetUsers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Addresses_AddressId",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "Address");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Address_AddressId",
                table: "Address",
                column: "AddressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Address",
                table: "Address",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Address_AddressId",
                table: "AspNetUsers",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
