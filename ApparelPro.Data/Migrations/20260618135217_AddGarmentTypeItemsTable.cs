using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGarmentTypeItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarmentTypeItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GarmentTypeId = table.Column<int>(type: "int", nullable: false),
                    StockCode = table.Column<string>(type: "varchar(2)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(8,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentTypeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentTypeItems_GarmentTypes_GarmentTypeId",
                        column: x => x.GarmentTypeId,
                        principalTable: "GarmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarmentTypeItems_GarmentTypeId",
                table: "GarmentTypeItems",
                column: "GarmentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarmentTypeItems");
        }
    }
}
