using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventsTrackingArchitectureTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StylewiseEvents",
                columns: table => new
                {
                    EventCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    Description = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StylewiseEvents", x => x.EventCode);
                });

            migrationBuilder.CreateTable(
                name: "EventMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(12)", nullable: false),
                    EventCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "date", nullable: true),
                    ActualDate = table.Column<DateTime>(type: "date", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventMasters_StylewiseEvents_EventCode",
                        column: x => x.EventCode,
                        principalTable: "StylewiseEvents",
                        principalColumn: "EventCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventMasters_BuyerCode_Order_TypeCode_StyleCode_EventCode",
                table: "EventMasters",
                columns: new[] { "BuyerCode", "Order", "TypeCode", "StyleCode", "EventCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventMasters_EventCode",
                table: "EventMasters",
                column: "EventCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventMasters");

            migrationBuilder.DropTable(
                name: "StylewiseEvents");
        }
    }
}
