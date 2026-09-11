using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLetterOfCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LetterOfCreditHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    LcNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    CreditNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryPlaceCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    OpeningDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BeneficiaryCode = table.Column<int>(type: "int", nullable: true),
                    IssuedBy = table.Column<string>(type: "varchar(1)", nullable: true),
                    NotifyPartyCode = table.Column<string>(type: "varchar(6)", nullable: true),
                    LicenceType = table.Column<string>(type: "varchar(6)", nullable: true),
                    LicenceNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    TransferableCredit = table.Column<string>(type: "varchar(1)", nullable: true),
                    ConfirmedCredit = table.Column<string>(type: "varchar(1)", nullable: true),
                    PartShipment = table.Column<string>(type: "varchar(1)", nullable: true),
                    Transhipment = table.Column<string>(type: "varchar(1)", nullable: true),
                    InsuranceCoverage = table.Column<string>(type: "varchar(1)", nullable: true),
                    ShipmentTerm = table.Column<string>(type: "varchar(1)", nullable: true),
                    ShipmentTermOther = table.Column<string>(type: "varchar(10)", nullable: true),
                    BillOfLadingIssued = table.Column<string>(type: "varchar(1)", nullable: true),
                    FreightPayment = table.Column<string>(type: "varchar(1)", nullable: true),
                    AirwayDocumentType = table.Column<string>(type: "varchar(1)", nullable: true),
                    AdditionalConditions = table.Column<string>(type: "varchar(1)", nullable: true),
                    ExtraConditions = table.Column<string>(type: "varchar(1)", nullable: true),
                    CreditBy = table.Column<string>(type: "varchar(1)", nullable: true),
                    BeneficiaryDraft = table.Column<string>(type: "varchar(1)", nullable: true),
                    InsuranceClause = table.Column<string>(type: "varchar(1)", nullable: true),
                    CountryOfOriginCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    ShipmentFromCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    TransportTo = table.Column<string>(type: "varchar(40)", nullable: true),
                    InsurancePercent = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    InsuranceValueCurrency = table.Column<string>(type: "varchar(3)", nullable: true),
                    CertifiedMailCopies = table.Column<string>(type: "varchar(3)", nullable: true),
                    DocumentPresentationDays = table.Column<string>(type: "varchar(3)", nullable: true),
                    ShipmentTermCustomLabel = table.Column<string>(type: "varchar(10)", nullable: true),
                    AccountNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    Branch = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreditAvailableWith = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreditDocuments = table.Column<string>(type: "varchar(10)", nullable: true),
                    ConformityWith = table.Column<string>(type: "varchar(50)", nullable: true),
                    InsuranceRemarks = table.Column<string>(type: "varchar(50)", nullable: true),
                    TenorDays = table.Column<string>(type: "varchar(3)", nullable: true),
                    DrawnOn = table.Column<string>(type: "varchar(20)", nullable: true),
                    CiCopies = table.Column<string>(type: "varchar(3)", nullable: true),
                    TenorDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NotLaterThanDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BeneficiaryCountryCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    AdvisingBankCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    InvoiceSelection = table.Column<string>(type: "varchar(1)", nullable: true),
                    PackingSpecification = table.Column<string>(type: "varchar(3)", nullable: true),
                    MarineBillOfLading = table.Column<string>(type: "varchar(40)", nullable: true),
                    MarineBillOfLadingConsignee = table.Column<string>(type: "varchar(40)", nullable: true),
                    AirWaybill = table.Column<string>(type: "varchar(40)", nullable: true),
                    AirWaybillConsignee = table.Column<string>(type: "varchar(40)", nullable: true),
                    OtherDocuments = table.Column<string>(type: "varchar(40)", nullable: true),
                    BankSentTo = table.Column<string>(type: "varchar(50)", nullable: true),
                    ImportPermitNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    ImportContractNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    IncomeTaxNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    BttReferenceNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    ImportValidityDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LetterOfCreditHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LetterOfCreditLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    LcNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(22)", nullable: false),
                    Description = table.Column<string>(type: "varchar(60)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    BtnNo = table.Column<string>(type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LetterOfCreditLines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LetterOfCreditHeaders_BankCode_LcNo",
                table: "LetterOfCreditHeaders",
                columns: new[] { "BankCode", "LcNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LetterOfCreditLines_BankCode_LcNo",
                table: "LetterOfCreditLines",
                columns: new[] { "BankCode", "LcNo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LetterOfCreditHeaders");

            migrationBuilder.DropTable(
                name: "LetterOfCreditLines");
        }
    }
}
