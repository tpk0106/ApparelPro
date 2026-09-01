using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonsAndYearSeasonReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.UniqueConstraint("AK_Seasons_Code", x => x.Code);
                });

            //migrationBuilder.InsertData(
            //    table: "Seasons",
            //    columns: new[] { "Id", "Code", "Description" },
            //    values: new object[,]
            //    {
            //        { 1, "WINTER", "WINTER" },
            //        { 2, "SPRING", "SPRING SEASON" },
            //        { 3, "HOLIDA", "HOLIDAY" },
            //        { 4, "FALL", "FALL" },
            //        { 5, "FA93", "FALL 1993" },
            //        { 6, "FA95", "FALL 1995" },
            //        { 7, "SUMR96", "SUMMER 1996" },
            //        { 8, "SPRI96", "SPRING SEASON 1996" }
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Season",
                table: "PurchaseOrders",
                column: "Season");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_Code",
                table: "Seasons",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Seasons_Season",
                table: "PurchaseOrders",
                column: "Season",
                principalTable: "Seasons",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Seasons_Season",
                table: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_Season",
                table: "PurchaseOrders");
        }
    }
}
