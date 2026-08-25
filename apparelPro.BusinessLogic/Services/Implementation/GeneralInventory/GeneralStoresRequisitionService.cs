using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Replicates GI_STRN1.PRG/GI_STRN2.PRG - General Inventory's Stores Requisition
    // Note. Unlike Orderwise's STRN, this is never tied to a Buyer/Order - Store and
    // Department are the only header-level scope, matching the legacy screen exactly
    // (SRN No / Date / From Stores / To Department, no Buyer/Order prompt at all).
    //
    // Legacy quirk NOT replicated: GI_STRN1.PRG never mutates gi_stmst on save - it
    // only checks (qty_in_hd - shdw_bal) and writes the gi_sttr ledger row. Per
    // explicit product decision (2026-08-25), this rebuild ALSO increments
    // GeneralStockMasters.ShadowBalance on commit, treating it as GI's equivalent of
    // Orderwise's dedicated StrnBalance lock field (GI's schema has only the one
    // balance column, so it does double duty here).
    public class GeneralStoresRequisitionService : IGeneralStoresRequisitionService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GeneralStoresRequisitionService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<GeneralStockItemAvailabilityServiceModel> VerifyStockItemAvailabilityAsync(string storeCode, string itemCode, string targetUnit)
        {
            storeCode = storeCode.Trim().ToUpper();
            itemCode = itemCode.Trim();
            targetUnit = targetUnit.Trim().ToUpper();

            var master = await _apparelProDbContext.GeneralStockMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.StoreCode == storeCode && m.ItemCode == itemCode);
            if (master == null) return null!;

            var reference = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ItemCode == itemCode);

            decimal convertedInHand = await _sharedService.ConvertUnitAsync(master.Unit, targetUnit, master.QtyInHand);
            decimal convertedShadow = await _sharedService.ConvertUnitAsync(master.Unit, targetUnit, master.ShadowBalance);

            return new GeneralStockItemAvailabilityServiceModel
            {
                ItemCode = itemCode,
                Description = reference?.Description ?? "",
                Unit = targetUnit,
                PhysicalQtyInHand = convertedInHand,
                ShadowAllocatedBalance = convertedShadow,
            };
        }

        public async Task<bool> CommitGeneralStoresRequisitionNoteAsync(
            GeneralRequisitionHeaderServiceModel header, List<GeneralRequisitionLineItemServiceModel> lines, string username)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Requisition cannot be committed without line items.");

            header.StoreCode = header.StoreCode.Trim().ToUpper();
            header.DepartmentCode = header.DepartmentCode.Trim().ToUpper();

            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == header.StoreCode);
            if (store == null)
                throw new InvalidOperationException("Invalid Stores Code.");

            var department = await _apparelProDbContext.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DepartmentCode == header.DepartmentCode);
            if (department == null)
                throw new InvalidOperationException("Invalid Department Code.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string allocatedSrnNumber = await _sharedService.GenerateNextDocumentNumberAsync("GSRN");
                header.SrnNumber = allocatedSrnNumber;

                foreach (var line in lines)
                {
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();

                    // 🔒 Same UPDLOCK/HOLDLOCK reasoning as Orderwise's StoresRequisitionService -
                    // this row's QtyInHand/ShadowBalance must be locked for the rest of this transaction.
                    var master = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {header.StoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();

                    if (master == null)
                        throw new InvalidOperationException($"Item not found in [Transfer From Stores] for item '{line.ItemCode}'.");

                    decimal requestedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, master.Unit, line.Quantity);
                    decimal netAvailable = master.QtyInHand - master.ShadowBalance;

                    if (requestedInMasterUnit > netAvailable)
                        throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for item '{line.ItemCode}'.");

                    decimal price = master.QtyInHand == 0 ? 0 : Math.Round(master.Value / master.QtyInHand, 2);

                    var trxLine = new GeneralStockTransaction
                    {
                        TransactionTypeCode = "0S",
                        DocumentNumber = header.SrnNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = header.StoreCode,
                        DepartmentCode = header.DepartmentCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = price,
                    };
                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(trxLine);

                    master.ShadowBalance += requestedInMasterUnit;
                    _apparelProDbContext.GeneralStockMasters.Update(master);
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

        public async Task<List<GeneralStockLookupRowServiceModel>> GetAvailableStockChoicesAsync(string storeCode)
        {
            storeCode = storeCode.Trim().ToUpper();

            var masters = await _apparelProDbContext.GeneralStockMasters
                .AsNoTracking()
                .Where(m => m.StoreCode == storeCode)
                .ToListAsync();

            var itemCodes = masters.Select(m => m.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return masters.Select(m => new GeneralStockLookupRowServiceModel
            {
                ItemCode = m.ItemCode,
                StoreCode = m.StoreCode,
                Unit = m.Unit,
                Description = descriptions.GetValueOrDefault(m.ItemCode, ""),
                QtyInHand = m.QtyInHand,
            }).OrderBy(r => r.ItemCode).ToList();
        }

        public async Task<GeneralStrnPrintDetailsServiceModel> GetGeneralStrnPrintDetailsAsync(string srnNumber)
        {
            srnNumber = srnNumber.Trim();

            var transactionRows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == srnNumber && t.TransactionTypeCode == "0S")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (transactionRows.Count == 0)
                throw new KeyNotFoundException($"SRN No '{srnNumber}' not found.");

            var firstRow = transactionRows[0];

            var itemCodes = transactionRows.Select(t => t.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            var storeDescription = await _apparelProDbContext.GeneralStores
                .AsNoTracking()
                .Where(s => s.Code == firstRow.StoreCode)
                .Select(s => s.Description)
                .FirstOrDefaultAsync();

            var lines = transactionRows.Select(t => new GeneralStrnPrintLineServiceModel
            {
                ItemCode = t.ItemCode,
                Description = descriptions.GetValueOrDefault(t.ItemCode, ""),
                Unit = t.Unit,
                Quantity = t.Quantity,
            }).ToList();

            return new GeneralStrnPrintDetailsServiceModel
            {
                Header = new GeneralStrnPrintHeaderServiceModel
                {
                    SrnNumber = srnNumber,
                    StoreCode = firstRow.StoreCode,
                    StoreDescription = storeDescription ?? firstRow.StoreCode,
                    DepartmentCode = firstRow.DepartmentCode ?? "",
                    TransactionDate = firstRow.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = lines,
            };
        }

        public async Task<List<GeneralStoreServiceModel>> GetStoresAsync()
        {
            return await _apparelProDbContext.GeneralStores
                .AsNoTracking()
                .OrderBy(s => s.Code)
                .Select(s => new GeneralStoreServiceModel { Code = s.Code, Description = s.Description })
                .ToListAsync();
        }
    }
}
