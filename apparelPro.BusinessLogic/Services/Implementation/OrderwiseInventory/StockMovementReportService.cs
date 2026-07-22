using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    public class StockMovementReportService : IStockMovementReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        // Placeholder TransactionType codes for movement types that have no note-type
        // module writing to OrderwiseStockTransactions yet (only "0S" STRN and "4I"/"GR"
        // GIN/GRN exist today — see OrderwiseStockTransaction.cs comments and
        // GoodsReceivedNoteService.cs). Proposed here to match the existing 2-char code
        // convention; confirm/align these with the real codes once Stock Transfer,
        // Supplier Return, and Adjustment Note features are built. Until then, every SUM
        // against these codes evaluates to 0 by construction (no rows exist with these
        // types) — which is exactly the agreed "placeholder zero" behavior for this report.
        private const string TransferInTypeCode = "TI";
        private const string TransferOutTypeCode = "TO";
        private const string SupplierReturnTypeCode = "SR";
        private const string AdjustmentTypeCode = "AJ";

        public StockMovementReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        // Single query, single round trip — replaces the legacy in_smve2.prg per-row
        // `seek` against od_sacc2 for the description lookup (N+1 fix) and folds in the
        // Damaged/Transfer/Return/Adjustment aggregates as LEFT JOINs so a line with no
        // matching rows in a source table still renders (with 0), matching the legacy
        // report's behavior of always printing every in_stmst row for the order.
        //
        // NOTE: OrderwiseStockMaster.ItemCode is the 22-char composite
        // (StockCode 2 + ItemCode 4 + Feature1-4 x4). StyleMaterialCostProfile already
        // stores that same composite decomposed into separate columns, so this substrings
        // the master's ItemCode back apart to join against it. TypeCode/StyleCode aren't
        // available on OrderwiseStockMaster, so — same as the legacy seek — the first
        // matching StyleMaterialCostProfile row wins when more than one Type/Style shares
        // the same item+features (flagged during Phase 1 design as a data-quality risk
        // worth a business-rules decision; unchanged here).
        //
        // This is a wide multi-way LEFT JOIN with substring-based join keys — verify the
        // generated SQL query plan against real data volumes before relying on it for
        // large orders; if EF Core's translation proves inefficient, the substring/GroupBy
        // pieces can be pre-materialized into temp lookups instead.
        // ===========================================================================
        // BuildLineQuery — real-world test against the actual DB surfaced a bug in the
        // original design below (both the query-syntax and lambda/method-syntax GroupJoin
        // versions, kept commented per your "don't delete" instruction, but marked
        // SUPERSEDED because neither one actually runs):
        //
        //   System.InvalidOperationException: The LINQ expression '...GroupJoin(...)...'
        //   could not be translated. Either rewrite the query in a form that can be
        //   translated, or switch to client evaluation explicitly...
        //
        // Root cause: EF Core can only translate a GroupJoin if the group it produces is
        // resolved in the SAME step — either flattened via SelectMany(...DefaultIfEmpty())
        // for a real SQL LEFT JOIN, or immediately reduced to a scalar in that same
        // resultSelector. The original design carried each raw group (styleGroup,
        // damagedGroup, transferInGroup, ...) forward inside a growing anonymous type
        // through several more chained .GroupJoin()/join-into calls before finally calling
        // .FirstOrDefault()/.Select() on it in the final projection — by then EF Core can
        // no longer represent that shape as SQL and throws.
        //
        // Fix: replace every GroupJoin with a per-row CORRELATED SUBQUERY instead — each
        // quantity/description is computed with its own Where(...).Sum()/FirstOrDefault()
        // scoped to the current master row, right inside the projection. EF Core translates
        // this reliably (as scalar subqueries), it's simpler to read than the 6-stage join,
        // and it preserves the exact same semantics as before: "first match wins" for the
        // Description lookup, "no matching rows = 0" for every quantity column. This report
        // is scoped to a single Buyer+Order (bounded row count), so N per-row subqueries is
        // not a performance concern here — if this pattern gets reused for something
        // unbounded, prefer a single upfront GroupBy+Sum per source table joined back with
        // SelectMany(...DefaultIfEmpty()) instead (that shape, unlike GroupJoin, does
        // translate).
        // ===========================================================================
        private IQueryable<StockMovementReportLineServiceModel> BuildLineQuery(int buyerCode, string order)
        {
            var styleProfiles = _apparelProDbContext.StyleMaterialCostProfiles.AsNoTracking();
            var stocks = _apparelProDbContext.OrderwiseStocks.AsNoTracking();
            var transactions = _apparelProDbContext.OrderwiseStockTransactions.AsNoTracking();

            // ---------------------------------------------------------------------------
            // SUPERSEDED — LINQ QUERY-SYNTAX VERSION (6-way GroupJoin). Does NOT run — see
            // the explanation above. Kept commented for reference only, per your request
            // not to delete join-query expressions. Do not uncomment as-is.
            // ---------------------------------------------------------------------------
            //
            // var masterRows = _apparelProDbContext.OrderwiseStockMasters
            //     .AsNoTracking()
            //     .Where(m => m.BuyerCode == buyerCode && m.Order == order);
            //
            // var damagedByItem = _apparelProDbContext.OrderwiseStocks
            //     .AsNoTracking()
            //     .Where(s => s.BuyerCode == buyerCode && s.Order == order)
            //     .GroupBy(s => s.ItemCode)
            //     .Select(g => new ItemQuantityTotal { ItemCode = g.Key, Total = g.Sum(s => s.DamagedQuantity) });
            //
            // var transferInByItem = TransactionSumByItem(buyerCode, order, TransferInTypeCode);
            // var transferOutByItem = TransactionSumByItem(buyerCode, order, TransferOutTypeCode);
            // var supplierReturnByItem = TransactionSumByItem(buyerCode, order, SupplierReturnTypeCode);
            // var adjustmentByItem = TransactionSumByItem(buyerCode, order, AdjustmentTypeCode);
            //
            // var joined =
            //     from master in masterRows
            //     join style in _apparelProDbContext.StyleMaterialCostProfiles.AsNoTracking()
            //         on new
            //         {
            //             master.BuyerCode,
            //             master.Order,
            //             StockCode = master.ItemCode.Substring(0, 2),
            //             ItemCode = master.ItemCode.Substring(2, 4),
            //             Feature1 = master.ItemCode.Substring(6, 4),
            //             Feature2 = master.ItemCode.Substring(10, 4),
            //             Feature3 = master.ItemCode.Substring(14, 4),
            //             Feature4 = master.ItemCode.Substring(18, 4),
            //         }
            //         equals new
            //         {
            //             style.BuyerCode,
            //             style.Order,
            //             style.StockCode,
            //             style.ItemCode,
            //             style.Feature1,
            //             style.Feature2,
            //             style.Feature3,
            //             style.Feature4,
            //         }
            //         into styleGroup
            //     join damaged in damagedByItem on master.ItemCode equals damaged.ItemCode into damagedGroup
            //     join transferIn in transferInByItem on master.ItemCode equals transferIn.ItemCode into transferInGroup
            //     join transferOut in transferOutByItem on master.ItemCode equals transferOut.ItemCode into transferOutGroup
            //     join supplierReturn in supplierReturnByItem on master.ItemCode equals supplierReturn.ItemCode into supplierReturnGroup
            //     join adjustment in adjustmentByItem on master.ItemCode equals adjustment.ItemCode into adjustmentGroup
            //     select new StockMovementReportLineServiceModel
            //     {
            //         ItemCode = master.ItemCode,
            //         Description = styleGroup.Select(s => s.Description).FirstOrDefault() ?? string.Empty,
            //         Unit = master.Unit,
            //         OrderedQuantity = master.OrderedQuantity,
            //         ReceivedQuantity = master.ReceivedQuantity,
            //         RequisitionedQuantity = master.RequisitionedQuantity,
            //         IssuedQuantity = master.IssuedQuantity,
            //         DamagedQuantity = damagedGroup.Select(d => d.Total).FirstOrDefault(),
            //         TransferInQuantity = transferInGroup.Select(t => t.Total).FirstOrDefault(),
            //         TransferOutQuantity = transferOutGroup.Select(t => t.Total).FirstOrDefault(),
            //         SupplierReturnQuantity = supplierReturnGroup.Select(t => t.Total).FirstOrDefault(),
            //         LastAdjustmentQuantity = adjustmentGroup.Select(t => t.Total).FirstOrDefault(),
            //     };
            //
            // return joined.Select(l => new StockMovementReportLineServiceModel
            // {
            //     ItemCode = l.ItemCode,
            //     Description = l.Description,
            //     Unit = l.Unit,
            //     OrderedQuantity = l.OrderedQuantity,
            //     ReceivedQuantity = l.ReceivedQuantity,
            //     RequisitionedQuantity = l.RequisitionedQuantity,
            //     IssuedQuantity = l.IssuedQuantity,
            //     DamagedQuantity = l.DamagedQuantity,
            //     TransferInQuantity = l.TransferInQuantity,
            //     TransferOutQuantity = l.TransferOutQuantity,
            //     SupplierReturnQuantity = l.SupplierReturnQuantity,
            //     LastAdjustmentQuantity = l.LastAdjustmentQuantity,
            //     BalanceQuantity = l.ReceivedQuantity - l.IssuedQuantity + l.TransferInQuantity
            //                        - l.TransferOutQuantity - l.DamagedQuantity - l.SupplierReturnQuantity
            //                        + l.LastAdjustmentQuantity,
            // });

            // ---------------------------------------------------------------------------
            // SUPERSEDED — LAMBDA/METHOD-SYNTAX VERSION (chained .GroupJoin(...)). Same
            // translation failure as the query-syntax version above. Kept commented for
            // reference only, per your request not to delete join expressions. Do not
            // uncomment as-is.
            // ---------------------------------------------------------------------------
            //
            // var joinedLambda = masterRows
            //     .GroupJoin(
            //         _apparelProDbContext.StyleMaterialCostProfiles.AsNoTracking(),
            //         master => new
            //         {
            //             master.BuyerCode,
            //             master.Order,
            //             StockCode = master.ItemCode.Substring(0, 2),
            //             ItemCode = master.ItemCode.Substring(2, 4),
            //             Feature1 = master.ItemCode.Substring(6, 4),
            //             Feature2 = master.ItemCode.Substring(10, 4),
            //             Feature3 = master.ItemCode.Substring(14, 4),
            //             Feature4 = master.ItemCode.Substring(18, 4),
            //         },
            //         style => new
            //         {
            //             style.BuyerCode,
            //             style.Order,
            //             style.StockCode,
            //             style.ItemCode,
            //             style.Feature1,
            //             style.Feature2,
            //             style.Feature3,
            //             style.Feature4,
            //         },
            //         (master, styleGroup) => new { master, styleGroup })
            //     .GroupJoin(
            //         damagedByItem,
            //         x => x.master.ItemCode,
            //         damaged => damaged.ItemCode,
            //         (x, damagedGroup) => new { x.master, x.styleGroup, damagedGroup })
            //     .GroupJoin(
            //         transferInByItem,
            //         x => x.master.ItemCode,
            //         transferIn => transferIn.ItemCode,
            //         (x, transferInGroup) => new { x.master, x.styleGroup, x.damagedGroup, transferInGroup })
            //     .GroupJoin(
            //         transferOutByItem,
            //         x => x.master.ItemCode,
            //         transferOut => transferOut.ItemCode,
            //         (x, transferOutGroup) => new { x.master, x.styleGroup, x.damagedGroup, x.transferInGroup, transferOutGroup })
            //     .GroupJoin(
            //         supplierReturnByItem,
            //         x => x.master.ItemCode,
            //         supplierReturn => supplierReturn.ItemCode,
            //         (x, supplierReturnGroup) => new { x.master, x.styleGroup, x.damagedGroup, x.transferInGroup, x.transferOutGroup, supplierReturnGroup })
            //     .GroupJoin(
            //         adjustmentByItem,
            //         x => x.master.ItemCode,
            //         adjustment => adjustment.ItemCode,
            //         (x, adjustmentGroup) => new StockMovementReportLineServiceModel
            //         {
            //             ItemCode = x.master.ItemCode,
            //             Description = x.styleGroup.Select(s => s.Description).FirstOrDefault() ?? string.Empty,
            //             Unit = x.master.Unit,
            //             OrderedQuantity = x.master.OrderedQuantity,
            //             ReceivedQuantity = x.master.ReceivedQuantity,
            //             RequisitionedQuantity = x.master.RequisitionedQuantity,
            //             IssuedQuantity = x.master.IssuedQuantity,
            //             DamagedQuantity = x.damagedGroup.Select(d => d.Total).FirstOrDefault(),
            //             TransferInQuantity = x.transferInGroup.Select(t => t.Total).FirstOrDefault(),
            //             TransferOutQuantity = x.transferOutGroup.Select(t => t.Total).FirstOrDefault(),
            //             SupplierReturnQuantity = x.supplierReturnGroup.Select(t => t.Total).FirstOrDefault(),
            //             LastAdjustmentQuantity = adjustmentGroup.Select(t => t.Total).FirstOrDefault(),
            //         });
            //
            // return joinedLambda.Select(l => new StockMovementReportLineServiceModel
            // {
            //     ItemCode = l.ItemCode,
            //     Description = l.Description,
            //     Unit = l.Unit,
            //     OrderedQuantity = l.OrderedQuantity,
            //     ReceivedQuantity = l.ReceivedQuantity,
            //     RequisitionedQuantity = l.RequisitionedQuantity,
            //     IssuedQuantity = l.IssuedQuantity,
            //     DamagedQuantity = l.DamagedQuantity,
            //     TransferInQuantity = l.TransferInQuantity,
            //     TransferOutQuantity = l.TransferOutQuantity,
            //     SupplierReturnQuantity = l.SupplierReturnQuantity,
            //     LastAdjustmentQuantity = l.LastAdjustmentQuantity,
            //     BalanceQuantity = l.ReceivedQuantity - l.IssuedQuantity + l.TransferInQuantity
            //                        - l.TransferOutQuantity - l.DamagedQuantity - l.SupplierReturnQuantity
            //                        + l.LastAdjustmentQuantity,
            // });

            // ---------------------------------------------------------------------------
            // ACTIVE — QUERY-SYNTAX VERSION using per-row correlated subqueries instead of
            // GroupJoin (kept commented — the lambda/method-syntax version right below is
            // the one actually wired up and returned, per your request to keep both forms).
            // ---------------------------------------------------------------------------
            //
            // var queryStyle =
            //     from master in _apparelProDbContext.OrderwiseStockMasters.AsNoTracking()
            //     where master.BuyerCode == buyerCode && master.Order == order
            //     select new StockMovementReportLineServiceModel
            //     {
            //         ItemCode = master.ItemCode,
            //         Description = (from s in styleProfiles
            //                        where s.BuyerCode == master.BuyerCode
            //                           && s.Order == master.Order
            //                           && s.StockCode == master.ItemCode.Substring(0, 2)
            //                           && s.ItemCode == master.ItemCode.Substring(2, 4)
            //                           && s.Feature1 == master.ItemCode.Substring(6, 4)
            //                           && s.Feature2 == master.ItemCode.Substring(10, 4)
            //                           && s.Feature3 == master.ItemCode.Substring(14, 4)
            //                           && s.Feature4 == master.ItemCode.Substring(18, 4)
            //                        select s.Description).FirstOrDefault() ?? string.Empty,
            //         Unit = master.Unit,
            //         OrderedQuantity = master.OrderedQuantity,
            //         ReceivedQuantity = master.ReceivedQuantity,
            //         RequisitionedQuantity = master.RequisitionedQuantity,
            //         IssuedQuantity = master.IssuedQuantity,
            //         DamagedQuantity = (from s in stocks
            //                            where s.BuyerCode == buyerCode && s.Order == order && s.ItemCode == master.ItemCode
            //                            select (decimal?)s.DamagedQuantity).Sum() ?? 0m,
            //         TransferInQuantity = (from t in transactions
            //                               where t.BuyerCode == buyerCode && t.Order == order
            //                                  && t.ItemCode == master.ItemCode && t.TransactionType == TransferInTypeCode
            //                               select (decimal?)t.Quantity).Sum() ?? 0m,
            //         TransferOutQuantity = (from t in transactions
            //                                where t.BuyerCode == buyerCode && t.Order == order
            //                                   && t.ItemCode == master.ItemCode && t.TransactionType == TransferOutTypeCode
            //                                select (decimal?)t.Quantity).Sum() ?? 0m,
            //         SupplierReturnQuantity = (from t in transactions
            //                                   where t.BuyerCode == buyerCode && t.Order == order
            //                                      && t.ItemCode == master.ItemCode && t.TransactionType == SupplierReturnTypeCode
            //                                   select (decimal?)t.Quantity).Sum() ?? 0m,
            //         LastAdjustmentQuantity = (from t in transactions
            //                                   where t.BuyerCode == buyerCode && t.Order == order
            //                                      && t.ItemCode == master.ItemCode && t.TransactionType == AdjustmentTypeCode
            //                                   select (decimal?)t.Quantity).Sum() ?? 0m,
            //     };
            //
            // return queryStyle.Select(l => new StockMovementReportLineServiceModel
            // {
            //     ItemCode = l.ItemCode,
            //     Description = l.Description,
            //     Unit = l.Unit,
            //     OrderedQuantity = l.OrderedQuantity,
            //     ReceivedQuantity = l.ReceivedQuantity,
            //     RequisitionedQuantity = l.RequisitionedQuantity,
            //     IssuedQuantity = l.IssuedQuantity,
            //     DamagedQuantity = l.DamagedQuantity,
            //     TransferInQuantity = l.TransferInQuantity,
            //     TransferOutQuantity = l.TransferOutQuantity,
            //     SupplierReturnQuantity = l.SupplierReturnQuantity,
            //     LastAdjustmentQuantity = l.LastAdjustmentQuantity,
            //     BalanceQuantity = l.ReceivedQuantity - l.IssuedQuantity + l.TransferInQuantity
            //                        - l.TransferOutQuantity - l.DamagedQuantity - l.SupplierReturnQuantity
            //                        + l.LastAdjustmentQuantity,
            // });

            // ---------------------------------------------------------------------------
            // ACTIVE — LAMBDA / METHOD-SYNTAX VERSION using per-row correlated subqueries.
            // This is the one actually wired up and returned.
            // ---------------------------------------------------------------------------
            var joinedLambda = _apparelProDbContext.OrderwiseStockMasters
                .AsNoTracking()
                .Where(master => master.BuyerCode == buyerCode && master.Order == order)
                .Select(master => new StockMovementReportLineServiceModel
                {
                    ItemCode = master.ItemCode,
                    // StyleMaterialCostProfiles.ItemCode is now the same 22-char composite as
                    // OrderwiseStockMaster.ItemCode (collapsed 2026-07-22 from separate
                    // StockCode/ItemCode/Feature1-4 columns) — a direct equality match, no more
                    // Substring decomposition needed. "First match wins" risk noted above still
                    // applies (TypeCode/StyleCode aren't available on OrderwiseStockMaster).
                    Description = styleProfiles
                        .Where(s => s.BuyerCode == master.BuyerCode
                                 && s.Order == master.Order
                                 && s.ItemCode == master.ItemCode)
                        .Select(s => s.Description)
                        .FirstOrDefault() ?? string.Empty,
                    Unit = master.Unit,
                    OrderedQuantity = master.OrderedQuantity,
                    ReceivedQuantity = master.ReceivedQuantity,
                    RequisitionedQuantity = master.RequisitionedQuantity,
                    IssuedQuantity = master.IssuedQuantity,
                    DamagedQuantity = stocks
                        .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.ItemCode == master.ItemCode)
                        .Sum(s => (decimal?)s.DamagedQuantity) ?? 0m,
                    TransferInQuantity = transactions
                        .Where(t => t.BuyerCode == buyerCode && t.Order == order && t.ItemCode == master.ItemCode && t.TransactionType == TransferInTypeCode)
                        .Sum(t => (decimal?)t.Quantity) ?? 0m,
                    TransferOutQuantity = transactions
                        .Where(t => t.BuyerCode == buyerCode && t.Order == order && t.ItemCode == master.ItemCode && t.TransactionType == TransferOutTypeCode)
                        .Sum(t => (decimal?)t.Quantity) ?? 0m,
                    SupplierReturnQuantity = transactions
                        .Where(t => t.BuyerCode == buyerCode && t.Order == order && t.ItemCode == master.ItemCode && t.TransactionType == SupplierReturnTypeCode)
                        .Sum(t => (decimal?)t.Quantity) ?? 0m,
                    LastAdjustmentQuantity = transactions
                        .Where(t => t.BuyerCode == buyerCode && t.Order == order && t.ItemCode == master.ItemCode && t.TransactionType == AdjustmentTypeCode)
                        .Sum(t => (decimal?)t.Quantity) ?? 0m,
                });

            // Balance is derived, never persisted — same convention already used for
            // OrderwiseStockMaster's own Issued/Received running totals (see that model's
            // comments). Formula: Received - Issued + TransferIn - TransferOut - Damaged
            // - SupplierReturn + LastAdjustment.
            return joinedLambda.Select(l => new StockMovementReportLineServiceModel
            {
                ItemCode = l.ItemCode,
                Description = l.Description,
                Unit = l.Unit,
                OrderedQuantity = l.OrderedQuantity,
                ReceivedQuantity = l.ReceivedQuantity,
                RequisitionedQuantity = l.RequisitionedQuantity,
                IssuedQuantity = l.IssuedQuantity,
                DamagedQuantity = l.DamagedQuantity,
                TransferInQuantity = l.TransferInQuantity,
                TransferOutQuantity = l.TransferOutQuantity,
                SupplierReturnQuantity = l.SupplierReturnQuantity,
                LastAdjustmentQuantity = l.LastAdjustmentQuantity,
                BalanceQuantity = l.ReceivedQuantity - l.IssuedQuantity + l.TransferInQuantity
                                   - l.TransferOutQuantity - l.DamagedQuantity - l.SupplierReturnQuantity
                                   + l.LastAdjustmentQuantity,
            });
        }

        // Superseded — only referenced by the commented-out GroupJoin versions of
        // BuildLineQuery above. Kept (not deleted) alongside them for reference; the
        // active query no longer calls this.
        private IQueryable<ItemQuantityTotal> TransactionSumByItem(int buyerCode, string order, string transactionType) =>
            _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.BuyerCode == buyerCode && t.Order == order && t.TransactionType == transactionType)
                .GroupBy(t => t.ItemCode)
                .Select(g => new ItemQuantityTotal { ItemCode = g.Key, Total = g.Sum(t => t.Quantity) });

        private class ItemQuantityTotal
        {
            public string ItemCode { get; set; } = null!;
            public decimal Total { get; set; }
        }

        public async Task<StockMovementReportHeaderServiceModel> GetStockMovementReportHeaderAsync(int buyerCode, string order)
        {
            order = order.Trim();

            // Mirrors legacy: "seek xbuyer+xorder" on od_po, "do error with 'Buyer/Order not found in P/O Master File'"
            var purchaseOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!purchaseOrderExists)
                throw new KeyNotFoundException($"Buyer/Order not found in P/O Master File (Buyer: {buyerCode}, Order: {order}).");

            var lines = await BuildLineQuery(buyerCode, order).ToListAsync();

            // Mirrors legacy: "seek xbuyer+xorder" on in_stmst, "do error with 'No Items Available for above Order'"
            if (lines.Count == 0)
                throw new InvalidOperationException($"No items available for above order (Buyer: {buyerCode}, Order: {order}).");

            // Legacy in_smve2.prg only ever printed the raw buyer code ("Buyer : "+xbuyer) —
            // this looks up the name so the modernized report/PDF reads better than the
            // legacy printout did.
            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync();

            return new StockMovementReportHeaderServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = !string.IsNullOrWhiteSpace(buyerName) ? buyerName : buyerCode.ToString(),
                Order = order,
                TotalLineItems = lines.Count,
                FullyReceivedCount = lines.Count(l => l.ReceivedQuantity >= l.OrderedQuantity),
                DamagedItemCount = lines.Count(l => l.DamagedQuantity > 0),
                ShortfallCount = lines.Count(l => l.ReceivedQuantity < l.OrderedQuantity),
            };
        }

        public async Task<PaginationResult<StockMovementReportLineServiceModel>> GetStockMovementReportLinesAsync(
            int buyerCode, string order, int pageSize, int currentPage,
            string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            order = order.Trim();
            var query = BuildLineQuery(buyerCode, order);

            if (!string.IsNullOrWhiteSpace(filterColumn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                // Strategy-mapped filter instead of a nested if/else chain.
                query = filterColumn.ToLowerInvariant() switch
                {
                    "itemcode" => query.Where(l => l.ItemCode.Contains(filterQuery)),
                    "description" => query.Where(l => l.Description.Contains(filterQuery)),
                    _ => query,
                };
            }

            query = !string.IsNullOrWhiteSpace(sortColumn)
                ? (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByColumnDescending(sortColumn)
                    : query.OrderByColumn(sortColumn))
                : query.OrderBy(l => l.ItemCode); // default order mirrors the legacy in_stms1 index walk

            var totalItems = await query.CountAsync();
            var items = await query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginationResult<StockMovementReportLineServiceModel>(
                pageSize, currentPage, totalItems, items, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<List<StockMovementReportLineServiceModel>> GetStockMovementReportLinesForPdfAsync(int buyerCode, string order)
        {
            order = order.Trim();
            return await BuildLineQuery(buyerCode, order).OrderBy(l => l.ItemCode).ToListAsync();
        }
    }
}
