using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_SRN1.PRG/GI_SRN2.PRG - "SUPPLIER RETURN NOTE
    // (General)". Two stock types: Regular (returning good stock to the supplier -
    // decrements QtyInHand/Value like a GIN) and Damaged (returning stock already
    // written off via a DGN - decrements only DamagedQuantity, since the physical stock
    // already left when it was marked damaged). No ShadowBalance reservation during
    // entry (atomic validate-then-commit, same convention as every other General
    // Inventory note type here), no gi_monst writes (aggregate live).
    //
    // Deliberate bug fix vs legacy, confirmed by reading the actual commit block: for
    // the Damaged leg, GI_SRN1.PRG's committed code is "repl dam_qty with dam_qty -
    // m_qty" where m_qty is a stale value left over from the Regular branch's own
    // calculation (never reassigned for the Damaged path) - the correct converted-qty
    // line exists right next to it but is commented out ("*repl dam_qty with
    // dam_qty-convert(temp->unit,unit,temp->qty)"). This is a disabled fix, not
    // intended behavior, so this port uses the correct converted-quantity subtraction
    // instead of replicating the stale-variable bug.
    public class GeneralSupplierReturnService : IGeneralSupplierReturnService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GeneralSupplierReturnService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<bool> CommitGeneralSupplierReturnNoteAsync(
            GeneralSrtnHeaderServiceModel header,
            List<GeneralSrtnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Supplier Return Note cannot be committed without line items.");

            if (header.StockType != GeneralSrtnStockType.Regular && header.StockType != GeneralSrtnStockType.Damaged)
                throw new ArgumentException($"Invalid Stock Type '{header.StockType}'.");

            header.StoreCode = header.StoreCode.Trim().ToUpper();
            bool isRegular = header.StockType == GeneralSrtnStockType.Regular;

            var storeExists = await _apparelProDbContext.GeneralStores.AsNoTracking().AnyAsync(s => s.Code == header.StoreCode);
            if (!storeExists)
                throw new InvalidOperationException($"Invalid Stores Code '{header.StoreCode}'.");

            var supplierExists = await _apparelProDbContext.Suppliers.AsNoTracking().AnyAsync(s => s.SupplierCode == header.SupplierCode);
            if (!supplierExists)
                throw new InvalidOperationException($"Invalid Supplier Code '{header.SupplierCode}'.");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string allocatedSrtnNumber = await _sharedService.GenerateNextDocumentNumberAsync("GSRTN");

                var generalMasters = new Dictionary<string, GeneralStockMaster>();

                // Pass 1: lock and validate every line before writing anything.
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

                    decimal requestedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);
                    decimal availableBalance = isRegular
                        ? generalMaster.QtyInHand - generalMaster.ShadowBalance
                        : generalMaster.DamagedQuantity;

                    if (requestedInMasterUnit > availableBalance)
                        throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for Item '{line.ItemCode}'.");

                    generalMasters[line.ItemCode] = generalMaster;
                }

                // Pass 2: write everything now that every line has passed.
                foreach (var line in lines)
                {
                    var generalMaster = generalMasters[line.ItemCode];
                    decimal qtyInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);
                    decimal avgPrice = generalMaster.QtyInHand == 0 ? 0 : Math.Round(generalMaster.Value / generalMaster.QtyInHand, 2);

                    if (isRegular)
                    {
                        generalMaster.QtyInHand -= qtyInMasterUnit;
                        generalMaster.Value -= Math.Round(qtyInMasterUnit * avgPrice, 2);
                    }
                    else
                    {
                        generalMaster.DamagedQuantity -= qtyInMasterUnit;
                    }
                    _apparelProDbContext.GeneralStockMasters.Update(generalMaster);

                    await _apparelProDbContext.GeneralStockTransactions.AddAsync(new GeneralStockTransaction
                    {
                        TransactionTypeCode = isRegular ? "7SR" : "7SD",
                        DocumentNumber = allocatedSrtnNumber,
                        TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                        TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                        StoreCode = header.StoreCode,
                        ItemCode = line.ItemCode,
                        Unit = line.Unit,
                        Quantity = line.Quantity,
                        Price = avgPrice,
                        Currency = generalMaster.Currency,
                        SupplierCode = header.SupplierCode.ToString(),
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

        public async Task<GeneralSrtnPrintDetailsServiceModel> GetGeneralSrtnPrintDetailsAsync(string srtnNumber)
        {
            srtnNumber = srtnNumber.Trim();

            var rows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == srtnNumber && (t.TransactionTypeCode == "7SR" || t.TransactionTypeCode == "7SD"))
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (rows.Count == 0)
                throw new KeyNotFoundException($"SRN No '{srtnNumber}' not found.");

            var first = rows[0];
            string stockType = first.TransactionTypeCode == "7SR" ? GeneralSrtnStockType.Regular : GeneralSrtnStockType.Damaged;

            var store = await _apparelProDbContext.GeneralStores.AsNoTracking().FirstOrDefaultAsync(s => s.Code == first.StoreCode);
            int supplierCode = int.TryParse(first.SupplierCode, out var parsedSupplierCode) ? parsedSupplierCode : 0;
            var supplier = await _apparelProDbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);

            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new GeneralSrtnPrintDetailsServiceModel
            {
                Header = new GeneralSrtnPrintHeaderServiceModel
                {
                    SrtnNumber = srtnNumber,
                    StoreCode = first.StoreCode,
                    StoreDescription = store?.Description ?? first.StoreCode,
                    SupplierCode = supplierCode,
                    SupplierName = supplier?.Name ?? "",
                    StockType = stockType,
                    TransactionDate = first.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = rows.Select(r => new GeneralSrtnPrintLineServiceModel
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
