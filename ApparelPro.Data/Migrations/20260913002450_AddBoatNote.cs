using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBoatNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoatNoteCargoLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    ContainerNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    SealNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    PackageQuantity = table.Column<string>(type: "varchar(30)", nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", nullable: false),
                    HsCode = table.Column<string>(type: "varchar(15)", nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    WeightUnit = table.Column<string>(type: "varchar(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoatNoteCargoLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoatNoteHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    BoatNoteNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    BoatNoteDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomsRegNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    CusDecRef = table.Column<string>(type: "varchar(30)", nullable: true),
                    CompanyAddressId = table.Column<int>(type: "int", nullable: false),
                    VesselName = table.Column<string>(type: "varchar(60)", nullable: true),
                    VoyageNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    PortOfLoadingCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    DischargePortCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomsOfficerStatus = table.Column<string>(type: "varchar(40)", nullable: true),
                    CustomsOfficerReference = table.Column<string>(type: "varchar(40)", nullable: true),
                    TerminalOperatorReleaseStatus = table.Column<string>(type: "varchar(40)", nullable: true),
                    TerminalOperatorReference = table.Column<string>(type: "varchar(40)", nullable: true),
                    ShipperAgentSignOffStatus = table.Column<string>(type: "varchar(40)", nullable: true),
                    ChaLicenseNo = table.Column<string>(type: "varchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoatNoteHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoatNoteHeaders_CompanyAddresses_CompanyAddressId",
                        column: x => x.CompanyAddressId,
                        principalTable: "CompanyAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoatNoteCargoLines_InvoiceNumber",
                table: "BoatNoteCargoLines",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_BoatNoteHeaders_CompanyAddressId",
                table: "BoatNoteHeaders",
                column: "CompanyAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_BoatNoteHeaders_InvoiceNumber",
                table: "BoatNoteHeaders",
                column: "InvoiceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoatNoteCargoLines");

            migrationBuilder.DropTable(
                name: "BoatNoteHeaders");
        }
    }
}
