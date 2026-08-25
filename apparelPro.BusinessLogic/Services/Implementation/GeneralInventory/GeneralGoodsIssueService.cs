using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Replicates GI_GIN1.PRG/GI_GIN2.PRG - General Inventory's Goods Issue Note, always
    // raised against exactly one Stores Requisition Note.
    //
    // Legacy quirk NOT replicated: GI_GIN1.PRG's interactive edit screen adds the SRN's
    // requested qty to gi_stmst.shdw_bal the moment the GIN screen opens, keeps it in sync
    // on every keystroke, then subtracts the final edited qty back out at save time - a net
    // zero effect only because legacy's SRN itself never touches shdw_bal. Since this
    // project's modern STRN deliberately DOES increment ShadowBalance on save (2026-08-25
    // decision), this rebuild's correct equivalent is: release exactly the SRN's original
    // reserved quantity from ShadowBalance, and decrement QtyInHand by whatever quantity the
    // user actually issues (which may differ from the original SRN request) - the natural
    // consequence of that earlier decision, not a new deviation.
    public class GeneralGoodsIssueService : IGeneralGoodsIssueService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GeneralGoodsIssueService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<GeneralGinStrnLookupResultServiceModel> GetIssuableStrnLinesAsync(string strnNumber)
        {
            strnNumber = strnNumber.Trim();

            var strnLines = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == strnNumber && t.TransactionTypeCode == "0S")
                .ToListAsync();

            if (strnLines.Count == 0)
                throw new KeyNotFoundException("Invalid Stores Requisition No.");

            var alreadyIssued = strnLines.FirstOrDefault(l => !string.IsNullOrEmpty(l.LinkedDocumentNumber));
            if (alreadyIssued != null)
                throw new InvalidOperationException($"GIN No :- {alreadyIssued.LinkedDocumentNumber} already raised for given SRN No.");

            var storeCode = strnLines[0].StoreCode;
            var departmentCode = strnLines[0].DepartmentCode ?? "";

            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == storeCode);

            var itemCodes = strnLines.Select(l => l.ItemCode).Distinct().ToList();
            var masters = await _apparelProDbContext.GeneralStockMasters
                .AsNoTracking()
                .Where(m => m.StoreCode == storeCode && itemCodes.Contains(m.ItemCode))
                .ToDictionaryAsync(m => m.ItemCode);
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            var lines = strnLines.Select(l =>
            {
                masters.TryGetValue(l.ItemCode, out var master);
                return new GeneralGinIssuableStrnLineServiceModel
                {
                    ItemCode = l.ItemCode,
                    Description = descriptions.GetValueOrDefault(l.ItemCode, ""),
                    Unit = l.Unit,
                    RequestedQuantity = l.Quantity,
                    QtyInHand = master?.QtyInHand ?? 0,
                    ShadowBalance = master?.ShadowBalance ?? 0,
                    MinStock = master?.MinStock ?? 0,
                };
            }).ToList();

            return new GeneralGinStrnLookupResultServiceModel
            {
                SrnNumber = strnNumber,
                StoreCode = storeCode,
                StoreDescription = store?.Description ?? storeCode,
                DepartmentCode = departmentCode,
                Lines = lines,
            };
        }

        public async Task<bool> CommitGeneralGoodsIssueNoteAsync(
            GeneralGinHeaderServiceModel header,
            List<GeneralGinLineItemServiceModel> lines,
            string username,
            bool minStockOverrideAuthorized,
            bool overrideMinStockCheck)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Goods Issue Note cannot be committed without line items.");

            header.SourceStrnNumber = header.SourceStrnNumber.Trim();

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                var strnLines = await _apparelProDbContext.GeneralStockTransactions
                    .FromSqlInterpolated($@"SELECT * FROM GeneralStockTransactions WITH (UPDLOCK, HOLDLOCK)
                        WHERE DocumentNumber = {header.SourceStrnNumber} AND TransactionTypeCode = '0S'")
                    .ToListAsync();

                if (strnLines.Count == 0)
                    throw new InvalidOperationException("Invalid Stores Requesition No.");
                var alreadyIssued = strnLines.FirstOrDefault(l => !string.IsNullOrEmpty(l.LinkedDocumentNumber));
                if (alreadyIssued != null)
                    throw new InvalidOperationException($"GIN No :- {alreadyIssued.LinkedDocumentNumber} already raised for given SRN No.");

                header.StoreCode = strnLines[0].StoreCode;
                header.DepartmentCode = strnLines[0].DepartmentCode ?? "";
                var strnByItem = strnLines.ToDictionary(l => l.ItemCode);

                string allocatedGinNumber = await _sharedService.GenerateNextDocumentNumberAsync("GGIN");

                // Pass 1: lock every affected GeneralStockMaster row and validate both
                // checks BEFORE writing anything - matches legacy's "validate then commit
                // all lines together" behaviour (its final ESC-confirm loop), just without
                // the intermediate live-edit reservation dance (see class remarks).
                var masters = new Dictionary<string, GeneralStockMaster>();
                foreach (var line in lines)
                {
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();

                    if (!strnByItem.TryGetValue(line.ItemCode, out var strnLine))
                        throw new InvalidOperationException($"Item '{line.ItemCode}' was not requisitioned on SRN '{header.SourceStrnNumber}'.");

                    var master = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {header.StoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();
                    if (master == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' does not exist in the stock master file.");

                    decimal issueQty = await _sharedService.ConvertUnitAsync(line.Unit, master.Unit, line.Quantity);
                    decimal originalReservedQty = await _sharedService.ConvertUnitAsync(strnLine.Unit, master.Unit, strnLine.Quantity);

                    // Hard block - never overridable, matches legacy's final "Cannot Issue
                    // below Quantity In Hand" abort (uses raw QtyInHand, not net of ShadowBalance).
                    if (master.QtyInHand - issueQty < master.MinStock)
                        throw new InvalidOperationException($"Cannot Issue below Quantity In Hand for item '{line.ItemCode}'.");

                    // Soft block - overridable by a Merchandiser Manager. Checked against
                    // ShadowBalance net of THIS SRN's own reservation (about to be released),
                    // so other pending SRNs still count but this one doesn't double-count itself.
                    decimal effectiveShadowBalance = master.ShadowBalance - originalReservedQty;
                    if (master.QtyInHand - effectiveShadowBalance - issueQty < master.MinStock)
                    {
                        if (!minStockOverrideAuthorized || !overrideMinStockCheck)
                            throw new MinStockOverrideRequiredException($"Minimum Stock reached for item '{line.ItemCode}'. Manager override required.");
                    }

                    masters[line.ItemCode] = master;
                }

                // Pass 2: write everything now that every line has passed.
                foreach (var line in lines)
                {
                    var master = masters[line.ItemCode];
                    var strnLine = strnByItem[line.ItemCode];

                    decimal issueQty = await _sharedService.ConvertUnitAsync(line.Unit, master.Unit, line.Quantity);
                    decimal originalReservedQty = await _sharedService.ConvertUnitAsync(strnLine.Unit, master.Unit, strnLine.Quantity);
                    decimal avgPrice = master.QtyInHand == 0 ? 0 : Math.Round(master.Value / master.QtyInHand, 4);

                    master.QtyInHand -= issueQty;
                    master.ShadowBalance -= originalReservedQty;
                    master.Value -= Math.Round(issueQty * avgPrice, 2);
                    _apparelProDbContext.GeneralStockMasters.Update(master);

                    var ginRow = new GeneralStockTransaction
                    {
                        TransactionTypeCode = "4I",
                        DocumentNumber = allocatedGinNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = header.StoreCode,
                        DepartmentCode = header.DepartmentCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = avgPrice,
                    };
                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(ginRow);

                    strnLine.LinkedDocumentNumber = allocatedGinNumber;
                    _apparelProDbContext.GeneralStockTransactions.Update(strnLine);
                }

                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return true;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<GeneralGinPrintDetailsServiceModel> GetGeneralGinPrintDetailsAsync(string ginNumber)
        {
            ginNumber = ginNumber.Trim();

            var rows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == ginNumber && t.TransactionTypeCode == "4I")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (rows.Count == 0)
                throw new KeyNotFoundException($"GIN No '{ginNumber}' not found.");

            var firstRow = rows[0];
            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == firstRow.StoreCode);

            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new GeneralGinPrintDetailsServiceModel
            {
                Header = new GeneralGinPrintHeaderServiceModel
                {
                    GinNumber = ginNumber,
                    StoreCode = firstRow.StoreCode,
                    StoreDescription = store?.Description ?? firstRow.StoreCode,
                    DepartmentCode = firstRow.DepartmentCode ?? "",
                    TransactionDate = firstRow.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = rows.Select(r => new GeneralGinPrintLineServiceModel
                {
                    ItemCode = r.ItemCode,
                    Description = descriptions.GetValueOrDefault(r.ItemCode, ""),
                    Unit = r.Unit,
                    Quantity = r.Quantity,
                }).ToList(),
            };
        }
    }
}
