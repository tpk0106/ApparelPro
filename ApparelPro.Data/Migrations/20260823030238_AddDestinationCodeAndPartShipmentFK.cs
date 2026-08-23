using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationCodeAndPartShipmentFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Destinations",
                table: "Destinations");

            migrationBuilder.DropIndex(
                name: "IX_Destinations_CountryCode",
                table: "Destinations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Destinations");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Destinations",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Destinations_Code",
                table: "Destinations",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Destinations",
                table: "Destinations",
                columns: new[] { "CountryCode", "Code" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Code", "Flag", "Id", "Name" },
                values: new object[] { "GB", null, 50, "United Kingdom" });

            migrationBuilder.InsertData(
                table: "Destinations",
                columns: new[] { "Code", "CountryCode", "DestinationName" },
                values: new object[,]
                {
                    { "BAL", "USA", "Baltimore" },
                    { "LON", "GB", "London" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartShipments_DestinationCode",
                table: "PartShipments",
                column: "DestinationCode");

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_Code",
                table: "Destinations",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PartShipments_Destinations_DestinationCode",
                table: "PartShipments",
                column: "DestinationCode",
                principalTable: "Destinations",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartShipments_Destinations_DestinationCode",
                table: "PartShipments");

            migrationBuilder.DropIndex(
                name: "IX_PartShipments_DestinationCode",
                table: "PartShipments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Destinations_Code",
                table: "Destinations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Destinations",
                table: "Destinations");

            migrationBuilder.DropIndex(
                name: "IX_Destinations_Code",
                table: "Destinations");

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumns: new[] { "Code", "CountryCode" },
                keyColumnTypes: new[] { "varchar(3)", "nvarchar(3)" },
                keyValues: new object[] { "LON", "GB" });

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumns: new[] { "Code", "CountryCode" },
                keyColumnTypes: new[] { "varchar(3)", "nvarchar(3)" },
                keyValues: new object[] { "BAL", "USA" });

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Code",
                keyValue: "GB");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Destinations");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Destinations",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Destinations",
                table: "Destinations",
                columns: new[] { "Id", "CountryCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_CountryCode",
                table: "Destinations",
                column: "CountryCode");
        }
    }
}
