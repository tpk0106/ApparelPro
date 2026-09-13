using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExportLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExportLicenseHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyAddressId = table.Column<int>(type: "int", nullable: false),
                    ApplicantType = table.Column<string>(type: "varchar(30)", nullable: true),
                    BusinessRegistrationNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    VatRegistrationNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    Telephone = table.Column<string>(type: "varchar(20)", nullable: true),
                    Fax = table.Column<string>(type: "varchar(20)", nullable: true),
                    Email = table.Column<string>(type: "varchar(60)", nullable: true),
                    ApplicantIdOfficeUse = table.Column<string>(type: "varchar(30)", nullable: true),
                    LicenseType = table.Column<string>(type: "varchar(20)", nullable: true),
                    ExchangeType = table.Column<string>(type: "varchar(20)", nullable: true),
                    CommercialType = table.Column<string>(type: "varchar(20)", nullable: true),
                    BankCode = table.Column<string>(type: "varchar(10)", nullable: true),
                    ModeOfPayment = table.Column<string>(type: "varchar(10)", nullable: true),
                    ModeOfTransportation = table.Column<string>(type: "varchar(20)", nullable: true),
                    Consignee1BuyerCode = table.Column<int>(type: "int", nullable: true),
                    Consignee2BuyerCode = table.Column<int>(type: "int", nullable: true),
                    PurposeOfExportation = table.Column<string>(type: "varchar(200)", nullable: true),
                    UseOfCommodity = table.Column<string>(type: "varchar(200)", nullable: true),
                    SignatoryDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportLicenseHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportLicenseHeaders_CompanyAddresses_CompanyAddressId",
                        column: x => x.CompanyAddressId,
                        principalTable: "CompanyAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExportLicenseLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExportLicenseHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemNo = table.Column<int>(type: "int", nullable: false),
                    HsNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    Description = table.Column<string>(type: "varchar(150)", nullable: false),
                    PackSize = table.Column<string>(type: "varchar(30)", nullable: true),
                    UnitCode = table.Column<string>(type: "varchar(6)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    Insurance = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    TotalCif = table.Column<decimal>(type: "decimal(14,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportLicenseLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportLicenseLines_ExportLicenseHeaders_ExportLicenseHeaderId",
                        column: x => x.ExportLicenseHeaderId,
                        principalTable: "ExportLicenseHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExportLicenseHeaders_CompanyAddressId",
                table: "ExportLicenseHeaders",
                column: "CompanyAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportLicenseLines_ExportLicenseHeaderId",
                table: "ExportLicenseLines",
                column: "ExportLicenseHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportLicenseLines");

            migrationBuilder.DropTable(
                name: "ExportLicenseHeaders");
        }
    }
}
