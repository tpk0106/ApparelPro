using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_SSTAT.PRG - "STOCK STATUS REPORT". Legacy read
    // B/F Balance, per-note-type monthly totals, and C/F Balance directly off a stored
    // gi_monst (monthly summary) row, incrementally maintained by every note's own
    // commit routine. This codebase deliberately never built a gi_monst equivalent
    // (established precedent: aggregate live from GeneralStockTransactions instead,
    // same as every other General Inventory report/screen here) - so this report
    // reconstructs the same numbers by chronologically REPLAYING every ledger
    // transaction for the store, in date order, from the beginning of time up to the
    // end of the requested month.
    //
    // Why a full replay rather than "current balance minus this month's net effect":
    // Stock Adjustment Note ("3A") SETS the balance to an absolute value rather than
    // adding/subtracting a delta - the same way legacy's own gi_stmst.qty_in_hd is
    // overwritten, not incremented, on a SAN commit. That makes the balance
    // non-invertible from a simple sum; the only correct way to know the balance at
    // any historical point is to walk every transaction in order up to that point.
    public class GeneralStockStatusReportService : IGeneralStockStatusReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        // The only transaction type codes that actually move (or, for "3A", replace)
        // QtyInHand - matches exactly the set of note types legacy's own gi_monst
        // columns track. STRN's "0S" is deliberately excluded: it only reserves
        // ShadowBalance, never touches QtyInHand, and legacy's gi_monst is never
        // updated by STRN either (no bf_bal/cf_bal write exists in GI_STRN1.PRG).
        private static readonly string[] MovementTypeCodes =
        {
            "0G", "4I", "1TG", "6TG", "1TO", "6TO", "2R", "7SR", "7SD", "6D", "3A"
        };

        public GeneralStockStatusReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<List<GeneralStockStatusReportLineServiceModel>> BuildLinesAsync(string storeCode, int month, int year)
        {
            var monthStart = new DateOnly(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Every ledger row for this store up to and including month end - chronological
            // order is what makes the running-balance replay below correct.
            var transactions = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.StoreCode == storeCode
                    && MovementTypeCodes.Contains(t.TransactionTypeCode)
                    && t.TransactionDate <= monthEnd)
                .OrderBy(t => t.ItemCode)
                .ThenBy(t => t.TransactionDate)
                .ThenBy(t => t.TransactionTime)
                .ThenBy(t => t.Id)
                .ToListAsync();

            var itemCodes = transactions.Select(t => t.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            var results = new List<GeneralStockStatusReportLineServiceModel>();

            foreach (var itemGroup in transactions.GroupBy(t => t.ItemCode))
            {
                // Mirrors legacy's "seek m_mmyy+m_store_cd" miss on gi_monst - an item with
                // no activity in the requested month simply isn't printed.
                bool hasActivityThisMonth = itemGroup.Any(t => t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd);
                if (!hasActivityThisMonth)
                    continue;

                decimal running = 0;
                decimal broughtForward = 0;
                bool broughtForwardCaptured = false;
                decimal totGrns = 0, totGins = 0, totGtnsIn = 0, totGtnsOut = 0, totRtns = 0, totSrns = 0, totDgns = 0;
                decimal? lastSan = null;
                string unit = itemGroup.First().Unit;

                foreach (var tx in itemGroup) // already chronological (see OrderBy above)
                {
                    if (!broughtForwardCaptured && tx.TransactionDate >= monthStart)
                    {
                        broughtForward = running;
                        broughtForwardCaptured = true;
                    }

                    bool isWithinMonth = tx.TransactionDate >= monthStart && tx.TransactionDate <= monthEnd;

                    switch (tx.TransactionTypeCode)
                    {
                        case "0G":
                            running += tx.Quantity;
                            if (isWithinMonth) totGrns += tx.Quantity;
                            break;
                        case "4I":
                            running -= tx.Quantity;
                            if (isWithinMonth) totGins += tx.Quantity;
                            break;
                        case "1TG":
                        case "1TO":
                            running += tx.Quantity;
                            if (isWithinMonth) totGtnsIn += tx.Quantity;
                            break;
                        case "6TG":
                        case "6TO":
                            running -= tx.Quantity;
                            if (isWithinMonth) totGtnsOut += tx.Quantity;
                            break;
                        case "2R":
                            running += tx.Quantity;
                            if (isWithinMonth) totRtns += tx.Quantity;
                            break;
                        case "7SR":
                        case "7SD":
                            running -= tx.Quantity;
                            if (isWithinMonth) totSrns += tx.Quantity;
                            break;
                        case "6D":
                            running -= tx.Quantity;
                            if (isWithinMonth) totDgns += tx.Quantity;
                            break;
                        case "3A":
                            running = tx.Quantity; // absolute set, not additive
                            if (isWithinMonth) lastSan = tx.Quantity;
                            break;
                    }

                    unit = tx.Unit;
                }

                results.Add(new GeneralStockStatusReportLineServiceModel
                {
                    ItemCode = itemGroup.Key,
                    Description = descriptions.GetValueOrDefault(itemGroup.Key, ""),
                    Unit = unit,
                    BroughtForwardBalance = broughtForward,
                    TotalGrns = totGrns,
                    TotalGins = totGins,
                    TotalGtnsIn = totGtnsIn,
                    TotalGtnsOut = totGtnsOut,
                    TotalRtns = totRtns,
                    TotalSrns = totSrns,
                    TotalDgns = totDgns,
                    LastSan = lastSan,
                    CarriedForwardBalance = running,
                });
            }

            return results.OrderBy(l => l.ItemCode).ToList();
        }

        public async Task<GeneralStockStatusReportHeaderServiceModel> GetHeaderAsync(string storeCode, int month, int year)
        {
            storeCode = storeCode.Trim().ToUpper();

            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == storeCode);
            if (store == null)
                throw new KeyNotFoundException($"Invalid Stores Code '{storeCode}'.");

            var lines = await BuildLinesAsync(storeCode, month, year);
            if (lines.Count == 0)
                throw new InvalidOperationException($"No Transactions for given Year/Month/Stores.");

            return new GeneralStockStatusReportHeaderServiceModel
            {
                StoreCode = storeCode,
                StoreDescription = store.Description,
                Month = month,
                Year = year,
                TotalLineItems = lines.Count,
            };
        }

        public async Task<List<GeneralStockStatusReportLineServiceModel>> GetLinesAsync(string storeCode, int month, int year)
        {
            storeCode = storeCode.Trim().ToUpper();
            return await BuildLinesAsync(storeCode, month, year);
        }
    }
}
