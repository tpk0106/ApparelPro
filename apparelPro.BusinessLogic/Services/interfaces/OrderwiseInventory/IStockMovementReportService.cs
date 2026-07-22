using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IStockMovementReportService
    {
        // Validates Buyer/Order exists on the P/O Master File and has at least one
        // in_stmst line, then returns KPI/header context (legacy in_smve2.prg's two
        // "do error" checks, translated to typed exceptions the controller maps to
        // HTTP status codes).
        Task<StockMovementReportHeaderServiceModel> GetStockMovementReportHeaderAsync(int buyerCode, string order);

        // Server-side paginated line grid, for the on-screen preview.
        Task<PaginationResult<StockMovementReportLineServiceModel>> GetStockMovementReportLinesAsync(
            int buyerCode, string order, int pageSize, int currentPage,
            string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);

        // Full, unpaginated line set in the legacy item-code order, for the PDF export —
        // guarantees the printed report and the on-screen grid are built from the exact
        // same query logic and can never drift apart.
        Task<List<StockMovementReportLineServiceModel>> GetStockMovementReportLinesForPdfAsync(int buyerCode, string order);
    }
}
