﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class GtnTraceabilityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CounterpartyBuyerCode",
                table: "OrderwiseStockTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CounterpartyOrder",
                table: "OrderwiseStockTransactions",
                type: "varchar(12)",
                nullable: true);

            // Idempotent seed: only insert the GTN sequence row if it doesn't already exist.
            // SharedService.GenerateNextDocumentNumberAsync throws if the NoteType row is missing,
            // so this row must exist before the first GTN can ever be raised.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM DocumentSequences WHERE NoteType = 'GTN')
                BEGIN
                    INSERT INTO DocumentSequences (NoteType, LastAllocatedNumber, Prefix)
                    VALUES ('GTN', 0, '')
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DocumentSequences WHERE NoteType = 'GTN'");

            migrationBuilder.DropColumn(
                name: "CounterpartyOrder",
                table: "OrderwiseStockTransactions");

            migrationBuilder.DropColumn(
                name: "CounterpartyBuyerCode",
                table: "OrderwiseStockTransactions");
        }
    }
}
