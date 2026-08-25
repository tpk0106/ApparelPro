using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_RTN1.PRG/GI_RTN2.PRG - "GOODS RETURN NOTE (General)".
    // Material returning from a Department back into a General store - the reverse of a
    // GIN, but not linked back to the originating GIN (legacy doesn't track that link
    // either; it only records which Department it came from, via the DepartmentCode
    // tie-back already established for STRN). Same "aggregate live, no gi_monst writes"
    // and atomic two-pass validate-then-commit conventions as every other General
    // Inventory note type in this codebase.
    public class GeneralGoodsReturnService : IGeneralGoodsReturnService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GeneralGoodsReturnService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<bool> CommitGeneralGoodsReturnNoteAsync(
            GeneralRtnHeaderServiceModel header,
            List<GeneralRtnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Goods Return Note cannot be committed without line items.");

            header.StoreCode = header.StoreCode.Trim().ToUpper();
            header.DepartmentCode = header.DepartmentCode.Trim().ToUpper();

            var storeExists = await _apparelProDbContext.GeneralStores.AsNoTracking().AnyAsync(s => s.Code == header.StoreCode);
            if (!storeExists)
                throw new InvalidOperationException($"Invalid Stores Code '{header.StoreCode}'.");

            var departmentExists = await _apparelProDbContext.Departments.AsNoTracking().AnyAsync(d => d.DepartmentCode == header.DepartmentCode);
            if (!departmentExists)
                throw new InvalidOperationException($"Invalid Department Code '{header.DepartmentCode}'.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string allocatedRtnNumber = await _sharedService.GenerateNextDocumentNumberAsync("GRTN");

                var generalMasters = new Dictionary<string, GeneralStockMaster>();

                // Pass 1: lock and validate every line before writing anything. Mirrors
                // legacy's "Item not found in [Transfer To Stores]" - existence only, no
                // balance ceiling (a return only ever increases stock).
                foreach (var line in lines)
                {
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                    var generalMaster = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {header.StoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();
                    if (generalMaster == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' not found in Stores '{header.StoreCode}'.");

                    generalMasters[line.ItemCode] = generalMaster;
                }

                // Pass 2: write everything now that every line has passed.
                foreach (var line in lines)
                {
                    var generalMaster = generalMasters[line.ItemCode];

                    decimal avgPrice = generalMaster.QtyInHand == 0 ? 0 : Math.Round(generalMaster.Value / generalMaster.QtyInHand, 2);
                    decimal qtyInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);

                    generalMaster.QtyInHand += qtyInMasterUnit;
                    generalMaster.Value += Math.Round(qtyInMasterUnit * avgPrice, 2);
                    _apparelProDbContext.GeneralStockMasters.Update(generalMaster);

                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(new GeneralStockTransaction
                    {
                        TransactionTypeCode = "2R",
                        DocumentNumber = allocatedRtnNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = header.StoreCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = avgPrice,
                        Currency = generalMaster.Currency,
                        DepartmentCode = header.DepartmentCode,
                    });
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

        public async Task<GeneralRtnPrintDetailsServiceModel> GetGeneralRtnPrintDetailsAsync(string rtnNumber)
        {
            rtnNumber = rtnNumber.Trim();

            var rows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == rtnNumber && t.TransactionTypeCode == "2R")
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (rows.Count == 0)
                throw new KeyNotFoundException($"RTN No '{rtnNumber}' not found.");

            var first = rows[0];

            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == first.StoreCode);
            var department = first.DepartmentCode != null
                ? await _apparelProDbContext.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DepartmentCode == first.DepartmentCode)
                : null;

            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new GeneralRtnPrintDetailsServiceModel
            {
                Header = new GeneralRtnPrintHeaderServiceModel
                {
                    RtnNumber = rtnNumber,
                    DepartmentCode = first.DepartmentCode ?? "",
                    DepartmentName = department?.Name ?? "",
                    StoreCode = first.StoreCode,
                    StoreDescription = store?.Description ?? first.StoreCode,
                    TransactionDate = first.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = rows.Select(r => new GeneralRtnPrintLineServiceModel
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
