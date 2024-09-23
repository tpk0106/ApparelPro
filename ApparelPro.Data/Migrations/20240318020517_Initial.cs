using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    BankCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TelephoneNos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SwiftCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    AddressId = table.Column<int>(type: "int", nullable: true),
                    LoanLimit = table.Column<decimal>(type: "money", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.BankCode);
                });

            migrationBuilder.CreateTable(
                name: "Basis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ValueAdd = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Basis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Buyers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    TelephoneNos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MobileNos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Flag = table.Column<byte[]>(type: "varbinary(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PODetails",
                columns: table => new
                {
                    Buyer = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Style = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    PONo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCode = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    RefNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OrderUnit = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OrderQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ExportDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LCNo = table.Column<string>(type: "nvarchar(23)", maxLength: 23, nullable: false),
                    Balance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PODetails", x => new { x.Buyer, x.Order, x.Type, x.Style });
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Styles",
                columns: table => new
                {
                    Buyer = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Order = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Style = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ExportBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SupplierReturn = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CustomerReturn = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Username = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ProductionEndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EstimateApprovalDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EstimateApprovalUserName = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Exported = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Styles", x => new { x.Buyer, x.Order, x.Type, x.Style });
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    AddressType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    Minor = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Currencies_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockItems",
                columns: table => new
                {
                    StockCode = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StockId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItems", x => new { x.StockCode, x.ItemCode });
                    table.ForeignKey(
                        name: "FK_StockItems_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CountryCode",
                table: "Addresses",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CountryCode",
                table: "Currencies",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_StockId",
                table: "StockItems",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Styles_Buyer_Order_Type_Style",
                table: "Styles",
                columns: new[] { "Buyer", "Order", "Type", "Style" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "Basis");

            migrationBuilder.DropTable(
                name: "Buyers");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "GarmentTypes");

            migrationBuilder.DropTable(
                name: "PODetails");

            migrationBuilder.DropTable(
                name: "StockItems");

            migrationBuilder.DropTable(
                name: "Styles");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Stocks");
        }
    }
}
