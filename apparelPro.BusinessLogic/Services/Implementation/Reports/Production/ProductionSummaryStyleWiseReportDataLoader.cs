using ApparelPro.Data;
using ApparelPro.Data.Models.Production;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Shared data-loading step for PR_MPRO1.PRG's "PRODUCTION SUMMARY - STYLE
    // WISE" report and its per-line Detailed companion - both group the same
    // date-range slice of DailyProductionEntries by Buyer/Order/Type/Style,
    // just shape the per-line rows differently. Kept here so a fix to one
    // (e.g. how descriptions/basis codes are resolved) doesn't have to be
    // duplicated across both services.
    internal static class ProductionSummaryStyleWiseReportDataLoader
    {
        public record StyleKey(int BuyerCode, string Order, int TypeCode, string StyleCode);

        public record LoadedData(
            List<Section> Sections,
            Section FinalSection,
            List<StyleKey> StyleKeys,
            Dictionary<int, string> BuyerNames,
            Dictionary<(int BuyerCode, string Order), string?> Descriptions,
            Dictionary<(int BuyerCode, string Order), string> BasisCodes,
            Dictionary<StyleKey, Style> Styles,
            List<DailyProductionEntry> Entries,
            List<ProductionLineAllocation> Allocations);

        public static async Task<LoadedData> LoadAsync(ApparelProDbContext db, DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
                throw new InvalidOperationException("Start Date must not be after End Date.");

            var sections = await db.Sections.AsNoTracking().OrderBy(s => s.Code).ToListAsync();
            var finalSection = sections.FirstOrDefault(s => s.IsFinal)
                ?? throw new InvalidOperationException("No Section is marked as Final.");

            var entries = await db.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            // Mirrors legacy's own "No entries for Given Date range" error box.
            if (entries.Count == 0)
                throw new InvalidOperationException(
                    $"No entries for given date range {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}.");

            var styleKeys = entries
                .Select(e => new StyleKey(e.BuyerCode, e.Order, e.TypeCode, e.StyleCode))
                .Distinct()
                .OrderBy(k => k.BuyerCode).ThenBy(k => k.Order).ThenBy(k => k.TypeCode).ThenBy(k => k.StyleCode)
                .ToList();

            var buyerCodes = styleKeys.Select(k => k.BuyerCode).Distinct().ToList();
            var orders = styleKeys.Select(k => k.Order).Distinct().ToList();

            var buyerNames = await db.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            // Superset fetched via translatable Contains(), then filtered
            // exactly in-memory against styleKeys - the same pattern used
            // by the other report services for composite-key lookups that
            // EF can't turn into a clean SQL join.
            var purchaseOrders = await db.PurchaseOrders
                .AsNoTracking()
                .Where(p => buyerCodes.Contains(p.BuyerCode) && orders.Contains(p.Order))
                .ToListAsync();
            var relevantOrderKeys = styleKeys.Select(k => (k.BuyerCode, k.Order)).ToHashSet();
            var descriptions = purchaseOrders
                .Where(p => relevantOrderKeys.Contains((p.BuyerCode, p.Order)))
                .ToDictionary(p => (p.BuyerCode, p.Order), p => p.Description);
            var basisCodes = purchaseOrders
                .Where(p => relevantOrderKeys.Contains((p.BuyerCode, p.Order)))
                .ToDictionary(p => (p.BuyerCode, p.Order), p => p.BasisCode);

            var allStyles = await db.Styles
                .AsNoTracking()
                .Where(s => buyerCodes.Contains(s.BuyerCode) && orders.Contains(s.Order))
                .ToListAsync();
            var styleKeySet = styleKeys.ToHashSet();
            var styles = allStyles
                .Where(s => styleKeySet.Contains(new StyleKey(s.BuyerCode, s.Order, s.TypeCode, s.StyleCode)))
                .ToDictionary(s => new StyleKey(s.BuyerCode, s.Order, s.TypeCode, s.StyleCode), s => s);

            var allAllocations = await db.ProductionLineAllocations
                .AsNoTracking()
                .Where(a => buyerCodes.Contains(a.BuyerCode) && orders.Contains(a.Order))
                .ToListAsync();
            var allocations = allAllocations
                .Where(a => styleKeySet.Contains(new StyleKey(a.BuyerCode, a.Order, a.TypeCode, a.StyleCode)))
                .ToList();

            return new LoadedData(sections, finalSection, styleKeys, buyerNames, descriptions, basisCodes, styles, entries, allocations);
        }
    }
}
