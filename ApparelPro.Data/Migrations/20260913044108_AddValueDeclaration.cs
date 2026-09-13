using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddValueDeclaration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ValueDeclarationHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "varchar(25)", nullable: true),
                    CompanyAddressId = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<string>(type: "varchar(4)", nullable: true),
                    OfficeCode = table.Column<string>(type: "varchar(10)", nullable: true),
                    SeriesLetter = table.Column<string>(type: "varchar(5)", nullable: true),
                    CusDecNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    ExporterName = table.Column<string>(type: "varchar(100)", nullable: false),
                    ExporterAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "varchar(25)", nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ImporterCompanyAddressId = table.Column<int>(type: "int", nullable: true),
                    IndentingAgentName = table.Column<string>(type: "varchar(100)", nullable: true),
                    IndentingAgentAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImporterVatNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    DeclarantVatNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    SalesContractNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    SalesContractDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalInvoiceValue = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    NatureOfTransaction = table.Column<string>(type: "varchar(60)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    TermsOfDeliveryCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    IsRelatedToSeller = table.Column<bool>(type: "bit", nullable: true),
                    WasValueInfluencedByRelationship = table.Column<bool>(type: "bit", nullable: true),
                    IsSaleSubjectToConditions = table.Column<bool>(type: "bit", nullable: true),
                    HasPreviousImportsLast3Months = table.Column<bool>(type: "bit", nullable: true),
                    PreviousImportsDetails = table.Column<string>(type: "varchar(200)", nullable: true),
                    BrokerageCommission = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    CostOfContainers = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    PackingCosts = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    CostOfGoodsSuppliedByBuyer = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    RoyaltiesLicenseFees = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    ProceedsToSeller = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    LoadingHandlingCharges = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    Insurance = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    OtherPayments = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    TermsOfPaymentCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    PortOfShipmentCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    AwbBlNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    AwbBlDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SignatoryName = table.Column<string>(type: "varchar(40)", nullable: true),
                    SignatoryTitle = table.Column<string>(type: "varchar(40)", nullable: true),
                    SignatoryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SignatoryCompanyName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ContinuationSheetsCount = table.Column<int>(type: "int", nullable: true),
                    AppraiserComments = table.Column<string>(type: "varchar(200)", nullable: true),
                    ScComments = table.Column<string>(type: "varchar(200)", nullable: true),
                    ValuationReferenceNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    CentralValuationEndorsement = table.Column<string>(type: "varchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueDeclarationHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValueDeclarationHeaders_CompanyAddresses_CompanyAddressId",
                        column: x => x.CompanyAddressId,
                        principalTable: "CompanyAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValueDeclarationHeaders_CompanyAddresses_ImporterCompanyAddressId",
                        column: x => x.ImporterCompanyAddressId,
                        principalTable: "CompanyAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ValueDeclarationLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValueDeclarationHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemNo = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(150)", nullable: false),
                    Brand = table.Column<string>(type: "varchar(40)", nullable: true),
                    Model = table.Column<string>(type: "varchar(40)", nullable: true),
                    Size = table.Column<string>(type: "varchar(20)", nullable: true),
                    CountryOfOriginCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    UnitCode = table.Column<string>(type: "varchar(6)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    HsCode = table.Column<string>(type: "varchar(15)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueDeclarationLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValueDeclarationLines_ValueDeclarationHeaders_ValueDeclarationHeaderId",
                        column: x => x.ValueDeclarationHeaderId,
                        principalTable: "ValueDeclarationHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ValueDeclarationHeaders_CompanyAddressId",
                table: "ValueDeclarationHeaders",
                column: "CompanyAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ValueDeclarationHeaders_ImporterCompanyAddressId",
                table: "ValueDeclarationHeaders",
                column: "ImporterCompanyAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ValueDeclarationHeaders_InvoiceNo",
                table: "ValueDeclarationHeaders",
                column: "InvoiceNo");

            migrationBuilder.CreateIndex(
                name: "IX_ValueDeclarationHeaders_InvoiceNumber",
                table: "ValueDeclarationHeaders",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ValueDeclarationLines_ValueDeclarationHeaderId",
                table: "ValueDeclarationLines",
                column: "ValueDeclarationHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValueDeclarationLines");

            migrationBuilder.DropTable(
                name: "ValueDeclarationHeaders");
        }
    }
}
