using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLetterOfCreditCoveringLetter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LetterOfCreditCoveringLetters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    LcNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    LetterDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExportLcNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    Value = table.Column<string>(type: "varchar(15)", nullable: true),
                    Item1 = table.Column<string>(type: "varchar(15)", nullable: true),
                    Item2 = table.Column<string>(type: "varchar(15)", nullable: true),
                    Box1Selected = table.Column<bool>(type: "bit", nullable: false),
                    Box2Selected = table.Column<bool>(type: "bit", nullable: false),
                    Attn1 = table.Column<string>(type: "varchar(20)", nullable: true),
                    Box3Selected = table.Column<bool>(type: "bit", nullable: false),
                    Attn2 = table.Column<string>(type: "varchar(20)", nullable: true),
                    Box4Selected = table.Column<bool>(type: "bit", nullable: false),
                    SampleLcNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    Box5Selected = table.Column<bool>(type: "bit", nullable: false),
                    Box6Selected = table.Column<bool>(type: "bit", nullable: false),
                    Box7Selected = table.Column<bool>(type: "bit", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    Box8Line1 = table.Column<string>(type: "varchar(60)", nullable: true),
                    Box8Line2 = table.Column<string>(type: "varchar(60)", nullable: true),
                    Box9Line1 = table.Column<string>(type: "varchar(60)", nullable: true),
                    Box9Line2 = table.Column<string>(type: "varchar(60)", nullable: true),
                    Box10Line1 = table.Column<string>(type: "varchar(60)", nullable: true),
                    Box10Line2 = table.Column<string>(type: "varchar(60)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LetterOfCreditCoveringLetters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LetterOfCreditCoveringLetters_BankCode_LcNo",
                table: "LetterOfCreditCoveringLetters",
                columns: new[] { "BankCode", "LcNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LetterOfCreditCoveringLetters");
        }
    }
}
