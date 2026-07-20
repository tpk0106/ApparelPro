using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixStockTransactionUniqueIndexToStoreCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The existing unique index used StockCode, which is already embedded as the first
            // 2 chars of the 22-char ItemCode — redundant on its own, and missing StoreCode, which
            // is what legacy IN_STTR actually uses for per-line uniqueness within one document
            // (confirmed: IN_GIN3.PRG seeks 'm_srnno+id+buyer+order+item_cd+store_cd'). Without this
            // fix, two lines for the same item pulled from two different stores/bases within one
            // document would incorrectly collide on the old constraint.
            migrationBuilder.DropIndex(
                name: "IX_OrderwiseStockTransactions_DocumentNumber_TransactionType_StockCode_ItemCode",
                table: "OrderwiseStockTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockTransactions_DocumentNumber_TransactionType_StoreCode_ItemCode",
                table: "OrderwiseStockTransactions",
                columns: new[] { "DocumentNumber", "TransactionType", "StoreCode", "ItemCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderwiseStockTransactions_DocumentNumber_TransactionType_StoreCode_ItemCode",
                table: "OrderwiseStockTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockTransactions_DocumentNumber_TransactionType_StockCode_ItemCode",
                table: "OrderwiseStockTransactions",
                columns: new[] { "DocumentNumber", "TransactionType", "StockCode", "ItemCode" },
                unique: true);
        }
    }
}
