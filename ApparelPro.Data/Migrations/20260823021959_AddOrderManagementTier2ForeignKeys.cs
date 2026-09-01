using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderManagementTier2ForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StyleCode",
                table: "PartShipments",
                type: "nvarchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)");

            migrationBuilder.AlterColumn<string>(
                name: "Order",
                table: "PartShipments",
                type: "nvarchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)");

            // ColorSizeDetails' composite PK includes Style/Order, and SQL Server refuses
            // ALTER COLUMN on a PK column without first dropping that constraint (unlike a
            // plain unique index, which EF's generator already handles automatically above).
            migrationBuilder.DropPrimaryKey(
                name: "PK_ColorSizeDetails",
                table: "ColorSizeDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Style",
                table: "ColorSizeDetails",
                type: "nvarchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)");

            migrationBuilder.AlterColumn<string>(
                name: "Order",
                table: "ColorSizeDetails",
                type: "nvarchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ColorSizeDetails",
                table: "ColorSizeDetails",
                columns: new[] { "Buyer", "Order", "Type", "Style", "Color", "Size" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Styles_Buyer_Order_Type_Style",
                table: "Styles",
                columns: new[] { "Buyer", "Order", "Type", "Style" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Basis_Code",
                table: "Basis",
                column: "Code");

            //migrationBuilder.InsertData(
            //    table: "Basis",
            //    columns: new[] { "Id", "Code", "Description" },
            //    values: new object[] { 9, "CMP", "CUT, MAKE & PACK" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BasisCode",
                table: "PurchaseOrders",
                column: "BasisCode");

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_CountryCode",
                table: "Destinations",
                column: "CountryCode");

            migrationBuilder.AddForeignKey(
                name: "FK_ColorSizeDetails_Styles_Buyer_Order_Type_Style",
                table: "ColorSizeDetails",
                columns: new[] { "Buyer", "Order", "Type", "Style" },
                principalTable: "Styles",
                principalColumns: new[] { "Buyer", "Order", "Type", "Style" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Destinations_Countries_CountryCode",
                table: "Destinations",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartShipments_Styles_BuyerCode_Order_TypeCode_StyleCode",
                table: "PartShipments",
                columns: new[] { "BuyerCode", "Order", "TypeCode", "StyleCode" },
                principalTable: "Styles",
                principalColumns: new[] { "Buyer", "Order", "Type", "Style" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Basis_BasisCode",
                table: "PurchaseOrders",
                column: "BasisCode",
                principalTable: "Basis",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Buyers_BuyerCode",
                table: "PurchaseOrders",
                column: "BuyerCode",
                principalTable: "Buyers",
                principalColumn: "BuyerCode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Styles_PurchaseOrders_Buyer_Order",
                table: "Styles",
                columns: new[] { "Buyer", "Order" },
                principalTable: "PurchaseOrders",
                principalColumns: new[] { "BuyerCode", "Order" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ColorSizeDetails_Styles_Buyer_Order_Type_Style",
                table: "ColorSizeDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Destinations_Countries_CountryCode",
                table: "Destinations");

            migrationBuilder.DropForeignKey(
                name: "FK_PartShipments_Styles_BuyerCode_Order_TypeCode_StyleCode",
                table: "PartShipments");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Basis_BasisCode",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Buyers_BuyerCode",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Styles_PurchaseOrders_Buyer_Order",
                table: "Styles");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Styles_Buyer_Order_Type_Style",
                table: "Styles");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_BasisCode",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_Destinations_CountryCode",
                table: "Destinations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Basis_Code",
                table: "Basis");

            migrationBuilder.DeleteData(
                table: "Basis",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.AlterColumn<string>(
                name: "StyleCode",
                table: "PartShipments",
                type: "varchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)");

            migrationBuilder.AlterColumn<string>(
                name: "Order",
                table: "PartShipments",
                type: "varchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ColorSizeDetails",
                table: "ColorSizeDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Style",
                table: "ColorSizeDetails",
                type: "varchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)");

            migrationBuilder.AlterColumn<string>(
                name: "Order",
                table: "ColorSizeDetails",
                type: "varchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ColorSizeDetails",
                table: "ColorSizeDetails",
                columns: new[] { "Buyer", "Order", "Type", "Style", "Color", "Size" });
        }
    }
}
