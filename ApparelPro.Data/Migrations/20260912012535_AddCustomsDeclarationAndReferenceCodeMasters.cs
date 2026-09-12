using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomsDeclarationAndReferenceCodeMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgreementCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(2)", nullable: false),
                    Description = table.Column<string>(type: "varchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgreementCodes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ClearanceOffices",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(4)", nullable: false),
                    Description = table.Column<string>(type: "varchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClearanceOffices", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "CommodityCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(8)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommodityCodes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "CustomsDeclarationAttachedDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CusNo = table.Column<string>(type: "varchar(15)", nullable: false),
                    DocNo = table.Column<string>(type: "varchar(3)", nullable: false),
                    DocTypeCode = table.Column<string>(type: "varchar(25)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsDeclarationAttachedDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomsDeclarationHeaders",
                columns: table => new
                {
                    CusNo = table.Column<string>(type: "varchar(15)", nullable: false),
                    ExporterCode = table.Column<string>(type: "varchar(11)", nullable: true),
                    BoiRegNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    ConsigneeCode = table.Column<string>(type: "varchar(11)", nullable: true),
                    NotifyPartyCode = table.Column<string>(type: "varchar(11)", nullable: true),
                    DeclarantCode = table.Column<string>(type: "varchar(11)", nullable: true),
                    ClearanceOfficeCode = table.Column<string>(type: "varchar(4)", nullable: true),
                    FrontierOfficeCode = table.Column<string>(type: "varchar(4)", nullable: true),
                    CountryOfConsignmentCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    LocationOfGoods = table.Column<string>(type: "varchar(10)", nullable: true),
                    CountryOfOriginCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    CountryOfDestinationCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    WarehouseNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    WarehousePeriod = table.Column<string>(type: "varchar(10)", nullable: true),
                    PrecedingDocNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    VoyageNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    VoyageDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BlAwbNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    PaymentTermCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    DeliveryTermCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    Vessel = table.Column<string>(type: "varchar(20)", nullable: true),
                    PortOfLoadingCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    TransportModeCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    PrepaymentAccountName = table.Column<string>(type: "varchar(20)", nullable: true),
                    PrepaymentAccountNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    PortOfDischargeCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    PlaceOfDeliveryCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    BankCode = table.Column<string>(type: "varchar(8)", nullable: true),
                    ReferenceNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    Remark1 = table.Column<string>(type: "varchar(60)", nullable: true),
                    Remark2 = table.Column<string>(type: "varchar(60)", nullable: true),
                    Remark3 = table.Column<string>(type: "varchar(60)", nullable: true),
                    Remark4 = table.Column<string>(type: "varchar(60)", nullable: true),
                    DeclarantName = table.Column<string>(type: "varchar(30)", nullable: true),
                    SubmittedByName = table.Column<string>(type: "varchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsDeclarationHeaders", x => x.CusNo);
                });

            migrationBuilder.CreateTable(
                name: "CustomsDeclarationLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CusNo = table.Column<string>(type: "varchar(15)", nullable: false),
                    Item = table.Column<string>(type: "varchar(6)", nullable: false),
                    CustomsProcedureCode = table.Column<string>(type: "varchar(4)", nullable: true),
                    CommodityCode = table.Column<string>(type: "varchar(8)", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SupplementaryUnitCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    SupplementaryQty = table.Column<decimal>(type: "decimal(11,2)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    Fob = table.Column<decimal>(type: "decimal(11,2)", nullable: true),
                    Freight = table.Column<decimal>(type: "decimal(11,2)", nullable: true),
                    Insurance = table.Column<decimal>(type: "decimal(11,2)", nullable: true),
                    Other = table.Column<decimal>(type: "decimal(11,2)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(12,4)", nullable: true),
                    CountryCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    LicenceNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    AgreementCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    QtyDeducted = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Value = table.Column<string>(type: "varchar(15)", nullable: true),
                    AnyOther = table.Column<string>(type: "varchar(15)", nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsDeclarationLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomsDeclarationLineTaxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CusNo = table.Column<string>(type: "varchar(15)", nullable: false),
                    Item = table.Column<string>(type: "varchar(6)", nullable: false),
                    TaxCode = table.Column<string>(type: "varchar(4)", nullable: true),
                    BaseCode = table.Column<string>(type: "varchar(3)", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(6,1)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(11,2)", nullable: true),
                    Exempted = table.Column<decimal>(type: "decimal(11,2)", nullable: true),
                    Payable = table.Column<decimal>(type: "decimal(11,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsDeclarationLineTaxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomsProcedureCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(4)", nullable: false),
                    Description = table.Column<string>(type: "varchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsProcedureCodes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocNo = table.Column<string>(type: "varchar(3)", nullable: false),
                    DocTypeCode = table.Column<string>(type: "varchar(25)", nullable: false),
                    Description = table.Column<string>(type: "varchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DutyTaxCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(4)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DutyTaxCodes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerms",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(3)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerms", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TaxBaseCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(3)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxBaseCodes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TransportModes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(2)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportModes", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarationAttachedDocuments_CusNo",
                table: "CustomsDeclarationAttachedDocuments",
                column: "CusNo");

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarationLines_CusNo",
                table: "CustomsDeclarationLines",
                column: "CusNo");

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarationLineTaxes_CusNo_Item",
                table: "CustomsDeclarationLineTaxes",
                columns: new[] { "CusNo", "Item" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_DocNo_DocTypeCode",
                table: "DocumentTypes",
                columns: new[] { "DocNo", "DocTypeCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgreementCodes");

            migrationBuilder.DropTable(
                name: "ClearanceOffices");

            migrationBuilder.DropTable(
                name: "CommodityCodes");

            migrationBuilder.DropTable(
                name: "CustomsDeclarationAttachedDocuments");

            migrationBuilder.DropTable(
                name: "CustomsDeclarationHeaders");

            migrationBuilder.DropTable(
                name: "CustomsDeclarationLines");

            migrationBuilder.DropTable(
                name: "CustomsDeclarationLineTaxes");

            migrationBuilder.DropTable(
                name: "CustomsProcedureCodes");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "DutyTaxCodes");

            migrationBuilder.DropTable(
                name: "PaymentTerms");

            migrationBuilder.DropTable(
                name: "TaxBaseCodes");

            migrationBuilder.DropTable(
                name: "TransportModes");
        }
    }
}
