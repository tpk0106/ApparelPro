using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPipelineStageHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderPipelineStageHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "nvarchar(12)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "nvarchar(12)", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPipelineStageHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPipelineStageHistories_BuyerCode_Order_TypeCode_StyleCode_Stage",
                table: "OrderPipelineStageHistories",
                columns: new[] { "BuyerCode", "Order", "TypeCode", "StyleCode", "Stage" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderPipelineStageHistories");
        }
    }
}
