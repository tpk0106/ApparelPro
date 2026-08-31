using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_SMOVE.PRG - "STOCK MOVEMENT REPORT (for a Range)".
    // Unlike GeneralStockStatusReportService (aggregated totals per item), this report
    // lists every individual GeneralStockTransactions row for one item/store within a
    // month, in chronological order, with a running balance recomputed on each line -
    // same chronological-replay approach and same reason (Stock Adjustment Note's "3A"
    // absolutely overwrites the balance rather than adding a delta, so only a full
    // replay from the beginning gives a correct historical balance).
    //
    // One deliberate deviation from GI_SMOVE.PRG: legacy uses code "5D" for Damaged
    // Goods Note, but every other legacy source and this codebase's own
    // GeneralDamagedGoodsService use "6D" - "5D" is treated as a stale/superseded code
    // and not matched here.
    public class GeneralStockMovementReportService : IGeneralStockMovementReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        private static readonly Dictionary<string, string> DocumentTypeDescriptions = new()
        {
            ["0G"] = "Goods Received Note",
            ["0S"] = "Stores Requisition Note",
            ["1TO"] = "Goods Transfer Note (Order)",
            ["1TG"] = "Goods Transfer Note (General)",
            ["2R"] = "Goods Return Note",
            ["3A"] = "Stock Adjustment Note",
            ["4I"] = "Goods Issue Note",
            ["6D"] = "Damaged Goods Note",
            ["6TO"] = "Goods Transfer Note (Order)",
            ["6TG"] = "Goods Transfer Note (General)",
            ["7SR"] = "Supplier Return Note (Regular)",
            ["7SD"] = "Supplier Return Note (Damaged)",
        };

        // Types that neither increase nor decrease QtyInHand directionally - shown with
        // Status "-" instead of "In"/"Out" (matches legacy's `id$'3A |7SD|0S '` check).
        private static readonly HashSet<string> NonDirectionalTypeCodes = new() { "3A", "7SD", "0S" };

        public GeneralStockMovementReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<(GeneralStockMovementReportHeaderServiceModel Header, List<GeneralStockMovementReportLineServiceModel> Lines)> BuildAsync(
            string storeCode, string itemCode, int month, int year)
        {
            storeCode = storeCode.Trim().ToUpper();
            itemCode = itemCode.Trim();

            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == storeCode);
            if (store == null)
                throw new KeyNotFoundException($"Invalid Stores Code '{storeCode}'.");

            var itemRef = await _apparelProDbContext.GeneralStockReferences.AsNoTracking().FirstOrDefaultAsync(r => r.ItemCode == itemCode);
            if (itemRef == null)
                throw new KeyNotFoundException($"Invalid Item Code '{itemCode}'.");

            var monthStart = new DateOnly(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var transactions = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.StoreCode == storeCode && t.ItemCode == itemCode && t.TransactionDate <= monthEnd)
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.TransactionTime)
                .ThenBy(t => t.Id)
                .ToListAsync();

            if (!transactions.Any(t => t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd))
                throw new InvalidOperationException("No transactions for Item in given month.");

            // Batch-resolve GTN(General) counterpart stores: our ledger only stores the
            // store each leg itself belongs to, not the other side, so the other side is
            // found via the sibling row sharing the same DocumentNumber+ItemCode on a
            // different StoreCode with the opposite TransactionTypeCode.
            var gtnDocNumbers = transactions
                .Where(t => t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd && (t.TransactionTypeCode == "1TG" || t.TransactionTypeCode == "6TG"))
                .Select(t => t.DocumentNumber)
                .Distinct()
                .ToList();

            var counterpartStoreByDocNumber = new Dictionary<string, string>();
            if (gtnDocNumbers.Count > 0)
            {
                var siblingRows = await _apparelProDbContext.GeneralStockTransactions
                    .AsNoTracking()
                    .Where(t => t.ItemCode == itemCode
                        && gtnDocNumbers.Contains(t.DocumentNumber)
                        && (t.TransactionTypeCode == "1TG" || t.TransactionTypeCode == "6TG")
                        && t.StoreCode != storeCode)
                    .ToListAsync();
                foreach (var row in siblingRows)
                    counterpartStoreByDocNumber[row.DocumentNumber] = row.StoreCode;

                var counterpartStoreCodes = counterpartStoreByDocNumber.Values.Distinct().ToList();
                var counterpartStoreDescriptions = await _apparelProDbContext.GeneralStores
                    .AsNoTracking()
                    .Where(s => counterpartStoreCodes.Contains(s.Code))
                    .ToDictionaryAsync(s => s.Code, s => s.Description);

                foreach (var docNumber in counterpartStoreByDocNumber.Keys.ToList())
                {
                    var code = counterpartStoreByDocNumber[docNumber];
                    counterpartStoreByDocNumber[docNumber] = counterpartStoreDescriptions.GetValueOrDefault(code, code);
                }
            }

            var departmentCodes = transactions
                .Where(t => !string.IsNullOrWhiteSpace(t.DepartmentCode))
                .Select(t => t.DepartmentCode!)
                .Distinct()
                .ToList();
            var departmentNames = departmentCodes.Count > 0
                ? await _apparelProDbContext.Departments.AsNoTracking()
                    .Where(d => departmentCodes.Contains(d.DepartmentCode))
                    .ToDictionaryAsync(d => d.DepartmentCode, d => d.Name)
                : new Dictionary<string, string>();

            decimal running = 0;
            decimal broughtForward = 0;
            bool broughtForwardCaptured = false;
            var lines = new List<GeneralStockMovementReportLineServiceModel>();

            foreach (var tx in transactions) // already chronological
            {
                if (!broughtForwardCaptured && tx.TransactionDate >= monthStart)
                {
                    broughtForward = running;
                    broughtForwardCaptured = true;
                }

                string sourceTarget = "";
                bool isIncoming = false;

                switch (tx.TransactionTypeCode)
                {
                    case "0G":
                        running += tx.Quantity;
                        isIncoming = true;
                        sourceTarget = $"S:{tx.SupplierCode} I:{tx.InvoiceNumber}";
                        break;
                    case "0S":
                        // Reservation only - never moves QtyInHand (see GeneralStockStatusReportService).
                        break;
                    case "1TO":
                        running += tx.Quantity;
                        isIncoming = true;
                        sourceTarget = $"B:{tx.BuyerCode} O:{tx.Order}";
                        break;
                    case "6TO":
                        running -= tx.Quantity;
                        sourceTarget = $"B:{tx.BuyerCode} O:{tx.Order}";
                        break;
                    case "1TG":
                        running += tx.Quantity;
                        isIncoming = true;
                        sourceTarget = $"S:{counterpartStoreByDocNumber.GetValueOrDefault(tx.DocumentNumber, "")}";
                        break;
                    case "6TG":
                        running -= tx.Quantity;
                        sourceTarget = $"S:{counterpartStoreByDocNumber.GetValueOrDefault(tx.DocumentNumber, "")}";
                        break;
                    case "2R":
                        running += tx.Quantity;
                        isIncoming = true;
                        sourceTarget = $"D:{departmentNames.GetValueOrDefault(tx.DepartmentCode ?? "", tx.DepartmentCode ?? "")}";
                        break;
                    case "3A":
                        running = tx.Quantity; // absolute set, not additive
                        break;
                    case "4I":
                        running -= tx.Quantity;
                        sourceTarget = $"D:{departmentNames.GetValueOrDefault(tx.DepartmentCode ?? "", tx.DepartmentCode ?? "")}";
                        break;
                    case "6D":
                        running -= tx.Quantity;
                        break;
                    case "7SR":
                        running -= tx.Quantity;
                        sourceTarget = $"S:{tx.SupplierCode}";
                        break;
                    case "7SD":
                        // Damaged supplier return only touches DamagedQuantity, not QtyInHand.
                        sourceTarget = $"S:{tx.SupplierCode}";
                        break;
                    default:
                        continue; // unrecognised legacy type code - skip rather than misreport
                }

                bool isWithinMonth = tx.TransactionDate >= monthStart && tx.TransactionDate <= monthEnd;
                if (!isWithinMonth)
                    continue; // only the month's own rows are listed; earlier rows only feed the running balance

                lines.Add(new GeneralStockMovementReportLineServiceModel
                {
                    TransactionDate = tx.TransactionDate,
                    TransactionTime = tx.TransactionTime,
                    TransactionTypeCode = tx.TransactionTypeCode,
                    DocumentTypeDescription = DocumentTypeDescriptions.GetValueOrDefault(tx.TransactionTypeCode, tx.TransactionTypeCode),
                    DocumentNumber = tx.DocumentNumber,
                    Status = NonDirectionalTypeCodes.Contains(tx.TransactionTypeCode) ? "-" : (isIncoming ? "In" : "Out"),
                    SourceTarget = sourceTarget,
                    Amount = tx.Quantity,
                    RunningBalance = running,
                });
            }

            var header = new GeneralStockMovementReportHeaderServiceModel
            {
                StoreCode = storeCode,
                StoreDescription = store.Description,
                ItemCode = itemCode,
                ItemDescription = itemRef.Description,
                Unit = transactions.LastOrDefault(t => t.TransactionDate <= monthEnd)?.Unit ?? "",
                Month = month,
                Year = year,
                BroughtForwardBalance = broughtForward,
                CarriedForwardBalance = running,
                TransactionCount = lines.Count,
            };

            return (header, lines);
        }

        public async Task<GeneralStockMovementReportHeaderServiceModel> GetHeaderAsync(string storeCode, string itemCode, int month, int year)
        {
            var (header, _) = await BuildAsync(storeCode, itemCode, month, year);
            return header;
        }

        public async Task<List<GeneralStockMovementReportLineServiceModel>> GetLinesAsync(string storeCode, string itemCode, int month, int year)
        {
            var (_, lines) = await BuildAsync(storeCode, itemCode, month, year);
            return lines;
        }
    }
}
