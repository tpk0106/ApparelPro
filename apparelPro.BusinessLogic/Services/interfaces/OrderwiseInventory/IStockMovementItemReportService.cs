using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IStockMovementItemReportService
    {
        /// <summary>
        /// Returns the item picker options for the given Buyer/Order (step 3 of the
        /// Buyer -> Order -> Item cascade). Mirrors legacy IN_SMVE1.PRG's restricted
        /// item lookup, which only offers items that exist against that Buyer/Order.
        /// </summary>
        Task<List<StockMovementItemOptionServiceModel>> GetAvailableItemsAsync(int buyerCode, string order);

        /// <summary>
        /// Returns the report header (Buyer/Order/Item identity, description, unit,
        /// order quantity, transaction count and closing balance). Throws
        /// KeyNotFoundException if the Buyer/Order or Item is invalid (mirrors legacy
        /// IN_SMVE1.PRG's "Invalid Buyer/Order/Item Code" validation halts), and
        /// InvalidOperationException if the item has no transactions (mirrors legacy's
        /// "No transactions found for this item" message).
        /// </summary>
        Task<StockMovementItemReportHeaderServiceModel> GetHeaderAsync(int buyerCode, string order, string itemCode);

        /// <summary>
        /// Returns one page of the chronological transaction ledger with a running
        /// balance column. The full ledger is always computed first (balance is
        /// inherently sequential/stateful) and then sliced in memory - paginating at
        /// the SQL level before computing the balance would produce wrong balances on
        /// page 2 onward.
        /// </summary>
        Task<PaginationResult<StockMovementItemReportLineServiceModel>> GetLinesAsync(
            int buyerCode, string order, string itemCode, int pageSize, int currentPage);

        /// <summary>
        /// Returns the full, unpaginated chronological transaction ledger for PDF export.
        /// </summary>
        Task<List<StockMovementItemReportLineServiceModel>> GetLinesForPdfAsync(int buyerCode, string order, string itemCode);
    }
}
