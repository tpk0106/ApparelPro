using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateOfOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateOfOriginHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    RefNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    CompanyAddressId = table.Column<int>(type: "int", nullable: false),
                    CountryOfOrigin = table.Column<string>(type: "varchar(3)", nullable: false),
                    PortOfLoading = table.Column<string>(type: "varchar(40)", nullable: true),
                    OtherRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompetentAuthorityName = table.Column<string>(type: "varchar(40)", nullable: true),
                    IssuePlace = table.Column<string>(type: "varchar(40)", nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RequestSubmittedBy = table.Column<string>(type: "varchar(40)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateOfOriginHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateOfOriginHeaders_CompanyAddresses_CompanyAddressId",
                        column: x => x.CompanyAddressId,
                        principalTable: "CompanyAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CertificateOfOriginLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: false),
                    ItemNo = table.Column<int>(type: "int", nullable: false),
                    ShippingMarks = table.Column<string>(type: "varchar(100)", nullable: false),
                    PackageTypeQuantity = table.Column<string>(type: "varchar(60)", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(100)", nullable: false),
                    HsCode = table.Column<string>(type: "varchar(15)", nullable: false),
                    NettWeight = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    GrossWeight = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateOfOriginLines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateOfOriginHeaders_CompanyAddressId",
                table: "CertificateOfOriginHeaders",
                column: "CompanyAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateOfOriginHeaders_InvoiceNumber",
                table: "CertificateOfOriginHeaders",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificateOfOriginLines_InvoiceNumber",
                table: "CertificateOfOriginLines",
                column: "InvoiceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateOfOriginHeaders");

            migrationBuilder.DropTable(
                name: "CertificateOfOriginLines");
        }
    }
}
