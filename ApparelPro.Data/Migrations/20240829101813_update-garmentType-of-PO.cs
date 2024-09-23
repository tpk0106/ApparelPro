using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class updategarmentTypeofPO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // below Fluent migration code commnted and edited manullay as cannot change
            // column of string to int type by fluent migrator - Drop and add manually
            
            //migrationBuilder.AlterColumn<int>(
            //    name: "GarmentType",
            //    table: "PurchaseOrders",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(20)",
            //    oldMaxLength: 20);

            migrationBuilder.DropColumn(
             name: "GarmentType",
             table: "PurchaseOrders");

            migrationBuilder.AddColumn<int>(
                name: "GarmentType",
                table: "PurchaseOrders",
                type: "int",
                defaultValue: 1,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GarmentType",
                table: "PurchaseOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
