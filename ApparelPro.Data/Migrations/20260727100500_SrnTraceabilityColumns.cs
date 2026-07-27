﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class SrnTraceabilityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierCode",
                table: "OrderwiseStockTransactions",
                type: "int",
                nullable: true);

            // Idempotent seed: only insert the SRN sequence row if it doesn't already exist.
            // SharedService.GenerateNextDocumentNumberAsync throws if the NoteType row is missing,
            // so this row must exist before the first SRN can ever be raised.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM DocumentSequences WHERE NoteType = 'SRN')
                BEGIN
                    INSERT INTO DocumentSequences (NoteType, LastAllocatedNumber, Prefix)
                    VALUES ('SRN', 0, '')
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DocumentSequences WHERE NoteType = 'SRN'");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "OrderwiseStockTransactions");
        }
    }
}
