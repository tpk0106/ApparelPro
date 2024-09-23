using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class currencyexchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyExchanges",
                columns: table => new
                {
                    BaseCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    QuoteCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeDate = table.Column<DateTime>(type: "date", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rate = table.Column<decimal>(type: "decimal(12,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurencyExchange", x => new { x.BaseCurrency, x.QuoteCurrency, x.ExchangeDate });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyExchanges");
        }
    }
}
