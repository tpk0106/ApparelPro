﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockAdjustmentNoteSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent seed: only insert the SAN sequence row if it doesn't already exist.
            // SharedService.GenerateNextDocumentNumberAsync throws if the NoteType row is
            // missing, so this row must exist before the first SAN can ever be raised.
            // No column changes needed — unlike every other note type, SAN's
            // OrderwiseStockMaster contribution ("LastAdjustmentQuantity") is already
            // computed live from OrderwiseStockTransactions (see StockMovementReportService),
            // not persisted as a running-total column.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM DocumentSequences WHERE NoteType = 'SAN')
                BEGIN
                    INSERT INTO DocumentSequences (NoteType, LastAllocatedNumber, Prefix)
                    VALUES ('SAN', 0, '')
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DocumentSequences WHERE NoteType = 'SAN'");
        }
    }
}
