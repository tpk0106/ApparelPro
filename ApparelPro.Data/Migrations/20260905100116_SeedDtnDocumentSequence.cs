using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDtnDocumentSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Direct Goods Transfer Note (DTN) is a newly-built note type (legacy
            // IN_DTN1.PRG/IN_DTN2.PRG) - SharedService.GenerateNextDocumentNumberAsync
            // requires a pre-existing DocumentSequences row for its NoteType before it can
            // allocate the first DTN number, same as every other note type's row.
            migrationBuilder.InsertData(
                table: "DocumentSequences",
                columns: new[] { "NoteType", "LastAllocatedNumber", "Prefix" },
                values: new object[] { "DTN", 0, "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DocumentSequences",
                keyColumn: "NoteType",
                keyValue: "DTN");
        }
    }
}
