using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackingListEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackingListCartonDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(12)", nullable: false),
                    NewOrder = table.Column<string>(type: "varchar(12)", nullable: false),
                    FromCartonNo = table.Column<int>(type: "int", nullable: false),
                    ToCartonNo = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "varchar(6)", nullable: false),
                    Size = table.Column<string>(type: "varchar(6)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NoOfCartons = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingListCartonDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackingListLines",
                columns: table => new
                {
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(12)", nullable: false),
                    NewOrder = table.Column<string>(type: "varchar(12)", nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingListLines", x => new { x.InvoiceNumber, x.BuyerCode, x.Order, x.TypeCode, x.StyleCode, x.NewOrder });
                });

            migrationBuilder.CreateTable(
                name: "PackingListStringDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(12)", nullable: false),
                    NewOrder = table.Column<string>(type: "varchar(12)", nullable: false),
                    BarNo = table.Column<int>(type: "int", nullable: false),
                    FromStringNo = table.Column<int>(type: "int", nullable: false),
                    ToStringNo = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "varchar(6)", nullable: false),
                    Size = table.Column<string>(type: "varchar(6)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingListStringDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackingListCartonDetails_InvoiceNumber_BuyerCode_Order_TypeCode_StyleCode_NewOrder",
                table: "PackingListCartonDetails",
                columns: new[] { "InvoiceNumber", "BuyerCode", "Order", "TypeCode", "StyleCode", "NewOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PackingListStringDetails_InvoiceNumber_BuyerCode_Order_TypeCode_StyleCode_NewOrder",
                table: "PackingListStringDetails",
                columns: new[] { "InvoiceNumber", "BuyerCode", "Order", "TypeCode", "StyleCode", "NewOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackingListCartonDetails");

            migrationBuilder.DropTable(
                name: "PackingListLines");

            migrationBuilder.DropTable(
                name: "PackingListStringDetails");
        }
    }
}
