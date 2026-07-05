using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    public class StoresRequisitionService : IStoresRequisitionService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IUnitConversionService _unitConversionService;
        private readonly ISharedService _sharedService;

        public StoresRequisitionService(ApparelProDbContext apparelProDbContext, 
            ISharedService sharedService,
            IUnitConversionService unitConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
            _unitConversionService = unitConversionService;
        }

        public async Task<StockItemAvailabilityDetails> VerifyStockItemAvailabilityAsync(
            int buyerCode, string order, string storeCode, string itemCode, string targetUnit)
        {
            storeCode = storeCode.Trim().ToUpper();
            itemCode = itemCode.Trim();
            targetUnit = targetUnit.Trim().ToUpper();

            // 1. Locate the item inside your confirmed OrderwiseStocks ledger pool table
            var stockRecord = await _apparelProDbContext.OrderwiseStocks
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode &&
                                          s.Order == order.Trim() &&
                                          s.StoreCode == storeCode &&
                                          s.ItemCode == itemCode);

            if (stockRecord == null) return null!;

            // 2. Fetch descriptions out of the global master stock items references case-insensitively
            var catalogItem = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ItemCode == itemCode);

            // 3. Convert quantities to the target viewport unit type dynamically using your dependency service
            decimal convertedInHand = await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, targetUnit, stockRecord.QtyInHand);
            decimal convertedShadow = await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, targetUnit, stockRecord.ShadowBalance);
            decimal convertedSrn = await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, targetUnit, stockRecord.SrnBalance);

            return new StockItemAvailabilityDetails
            {
                ItemCode = itemCode,
                Description = catalogItem?.Description ?? "Raw Inventory Component Material",
                Unit = targetUnit,
                PhysicalQtyInHand = convertedInHand,
                ShadowAllocatedBalance = convertedShadow,
                RequisitionedSrnBalance = convertedSrn
            };
        }
        public async Task<bool> CommitStoresRequisitionNoteAsync(
            RequisitionHeaderServiceModel header, List<RequisitionLineItemServiceModel> lines, string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Requisition cannot be committed without line items.");

            // Initiate a secure EF Core transactional context lock
            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    header.Order = header.Order.Trim();
                    header.DepartmentCode = header.DepartmentCode.Trim().ToUpper();

                    // 1. THREAD-SAFE AUTOGEN EMULATION: Allocate a unique consecutive serial number for this STRN note
                    string allocatedSrnNumber = await _sharedService.GenerateNextDocumentNumberAsync("STRN");
                    header.SrnNumber = allocatedSrnNumber;

                    foreach (var line in lines)
                    {
                        line.StockCode = line.StockCode.Trim(); // Keeps your UI category prefix reference clean
                        line.ItemCode = line.ItemCode.Trim();
                        line.StoreCode = line.StoreCode.Trim().ToUpper(); // Enforces the explicit "Basis" Code string
                        line.Unit = line.Unit.Trim().ToUpper();

                        // 2. VERIFY AVAILABLE STOCK BALANCES IN THE CORRECT TABLE (OrderwiseStocks)
                        // 🔒 CONCURRENCY FIX: Read WITH (UPDLOCK, HOLDLOCK) so this row is exclusively locked for the
                        // remainder of this transaction. Without this, two concurrent STRN commits against the same
                        // item/store can both read the same QtyInHand/ShadowBalance/SrnBalance, both pass the deficit
                        // check below, and the second SaveChangesAsync silently overwrites (rather than adds to) the
                        // first commit's SrnBalance update — a classic lost-update race that lets stock be over-allocated.
                        // This is the direct modern equivalent of the legacy RLOCK()/FLOCK() the Clipper code relied on.
                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (stockRecord == null)
                        {
                            throw new InvalidOperationException($"Inventory Error: Item '{line.ItemCode}' under basis '{line.StoreCode}' does not exist in the stock master file.");
                        }

                        // Calculate net available inventory in the database record unit format using your injected service
                        decimal requestedInStockUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.Quantity);
                        decimal netAvailableInStockUnit = stockRecord.QtyInHand - stockRecord.ShadowBalance - stockRecord.SrnBalance;

                        if (requestedInStockUnit > netAvailableInStockUnit)
                        {
                            throw new InvalidOperationException($"Deficit Block: Attempted to exceed balance quantity for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Available: {await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, line.Unit, netAvailableInStockUnit)} {line.Unit}.");
                        }

                        // 3. WRITE TO THE TRANSACTION LEDGER RECORD TABLE (OrderwiseStockTransactions)
                        var trxLine = new OrderwiseStockTransaction
                        {
                            DocumentNumber = header.SrnNumber,
                            TransactionType = "0S", // '0S' = Stores Requisition Note
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                            DepartmentCode = header.DepartmentCode,
                            StockCode = line.StockCode,
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(trxLine);

                        // 4. UPDATE MASTER ITEM CATALOG VALUES (OrderwiseStockMasters)
                        // 🔒 Same UPDLOCK/HOLDLOCK reasoning as above: this row's RequisitionedQuantity is a running
                        // total that two concurrent commits could otherwise stomp on.
                        var catalogItem = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (catalogItem != null)
                        {
                            // Converts requested pieces back to your master catalog base unit safely
                            decimal requestedInCatalogUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, catalogItem.Unit ?? "PCS", line.Quantity);

                            // FIXED: 100% pure, type-safe C# assignment with no compilation tricks!
                            catalogItem.RequisitionedQuantity += requestedInCatalogUnit;

                            _apparelProDbContext.OrderwiseStockMasters.Update(catalogItem);
                        }

                        // 5. UPDATE LOCK WITHIN THE MASTER LEDGER POOL BALANCE (OrderwiseStocks)
                        stockRecord.SrnBalance += requestedInStockUnit; // Locks the allocation balance!
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);
                    }

                    await _apparelProDbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        //public async Task<List<OrderwiseStockLookupRowServiceModel>> GetAvailableStockChoicesAsync(int buyerCode, string order)
        //{
        //    order = order.Trim();

        //    // 1. Fetch all stock ledger balance records for this order scope
        //    var stockRecords = await _apparelProDbContext.OrderwiseStocks
        //        .AsNoTracking()
        //        .Where(s => s.BuyerCode == buyerCode && s.Order == order)
        //        .ToListAsync();

        //    var resultList = new List<OrderwiseStockLookupRowServiceModel>();

        //    // 2. Loop and enrich each row with its matching catalog description case-insensitively
        //    foreach (var stock in stockRecords)
        //    {
        //        var catalogItem = await _apparelProDbContext.StockItems
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(c => c.ItemCode == stock.ItemCode);

        //        resultList.Add(new OrderwiseStockLookupRowServiceModel
        //        {
        //            ItemCode = stock.ItemCode,
        //            StoreCode = stock.StoreCode,
        //            Unit = stock.Unit,
        //            Description = catalogItem?.Description ?? "Raw Material Component"
        //        });
        //    }

        //    return resultList.OrderBy(r => r.ItemCode).ToList();
        //}

        public async Task<List<OrderwiseStockLookupRowServiceModel>> GetAvailableStockChoicesAsync(int buyerCode, string order, string storeCode)
        {
            order = order.Trim();
            storeCode = storeCode.Trim().ToUpper();

            // FIXED: Strictly filter the stock pool down to the selected Buyer, Order, AND Store/Basis combination!
            var stockRecords = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.StoreCode == storeCode)
                .ToListAsync();

            // PERFORMANCE FIX: Batch-load every matching catalog description in a single round trip instead of
            // issuing one StockItems query per stock row (N+1 query pattern).
            var itemCodes = stockRecords.Select(s => s.ItemCode).Distinct().ToList();
            var catalogItemsByCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => itemCodes.Contains(c.ItemCode))
                .ToDictionaryAsync(c => c.ItemCode, c => c.Description);

            var resultList = stockRecords.Select(stock => new OrderwiseStockLookupRowServiceModel
            {
                ItemCode = stock.ItemCode,
                StoreCode = stock.StoreCode,
                Unit = stock.Unit,
                Description = catalogItemsByCode.TryGetValue(stock.ItemCode, out var description)
                    ? description ?? "Raw Material Component"
                    : "Raw Material Component"
            }).ToList();

            return resultList.OrderBy(r => r.ItemCode).ToList();
        }
    }
}



//using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
//using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
//using ApparelPro.Data;
//using ApparelPro.Data.Models.OrderwiseInventory;
//using Microsoft.EntityFrameworkCore;

//namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
//{
//    public class StoresRequisitionService : IStoresRequisitionService
//    {
//        private readonly ApparelProDbContext _apparelProDbContext;
//        private readonly IUnitConversionService _unitConversionService;

//        public StoresRequisitionService(ApparelProDbContext apparelProDbContext, IUnitConversionService unitConversionService)
//        {
//            _apparelProDbContext = apparelProDbContext;
//            _unitConversionService = unitConversionService;
//        }

//        public async Task<StockItemAvailabilityDetails> VerifyStockItemAvailabilityAsync(
//            int buyerCode, string order, string storeCode, string itemCode, string targetUnit)
//        {
//            storeCode = storeCode.Trim();
//            itemCode = itemCode.Trim();
//            storeCode  = storeCode.Trim();
//            targetUnit = targetUnit.Trim().ToUpper();

//            // 1. Locate the item inside your confirmed OrderwiseStocks ledger pool table
//            var stockRecord = await _apparelProDbContext.OrderwiseStocks
//                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode &&
//                                          s.Order == order.Trim() &&
//                                          s.StoreCode == storeCode &&
//                                          s.ItemCode == itemCode);

//            if (stockRecord == null) return null!;

//            // 2. Fetch descriptions out of the master stock items catalog references (combining codes case-insensitively)
//            var catalogItem = await _apparelProDbContext.StockItems
//                .AsNoTracking()
//                .FirstOrDefaultAsync(c => c.StockCode == storeCode && c.ItemCode == itemCode);

//            // 3. Convert quantities to the target viewport unit type dynamically using your strict conversion rules
//            decimal convertedInHand = await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, targetUnit, stockRecord.QtyInHand);
//            decimal convertedShadow = await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, targetUnit, stockRecord.ShadowBalance);
//            decimal convertedSrn = await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, targetUnit, stockRecord.SrnBalance);

//            return new StockItemAvailabilityDetails
//            {
//                ItemCode = $"{storeCode}{itemCode}",
//                Description = catalogItem?.Description ?? "Raw Inventory Component Material",
//                Unit = targetUnit,
//                PhysicalQtyInHand = convertedInHand,
//                ShadowAllocatedBalance = convertedShadow,
//                RequisitionedSrnBalance = convertedSrn
//            };
//        }

//        public async Task<bool> CommitStoresRequisitionNoteAsync(
//            RequisitionHeaderServiceModel header, List<RequisitionLineItemServiceModel> lines, string username)
//        {
//            if (lines == null || !lines.Any())
//                throw new ArgumentException("Transaction Aborted: Requisition cannot be committed without line items.");

//            // Initiate a secure EF Core transactional context lock
//            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
//            {
//                try
//                {
//                    header.Order = header.Order.Trim();
//                    header.DepartmentCode = header.DepartmentCode.Trim().ToUpper();

//                    // 1. THREAD-SAFE AUTOGEN EMULATION: Allocate a unique consecutive serial number for this SRN note
//                    string allocatedSrnNumber = await GenerateNextDocumentNumberAsync("SRN");
//                    header.SrnNumber = allocatedSrnNumber;

//                    foreach (var line in lines)
//                    {
//                        line.StockCode = line.StockCode.Trim();
//                        line.ItemCode = line.ItemCode.Trim();
//                        line.StockCode = line.StockCode.Trim().ToUpper();
//                        line.Unit = line.Unit.Trim().ToUpper();

//                        // 2. VERIFY AVAILABLE STOCK BALANCES IN THE CORRECT TABLE (OrderwiseStocks)
//                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
//                            .FirstOrDefaultAsync(s => s.BuyerCode == header.BuyerCode &&
//                                                      s.Order == header.Order &&
//                                                      s.StoreCode == line.StockCode &&
//                                                      s.ItemCode == line.ItemCode);

//                        if (stockRecord == null)
//                        {
//                            throw new InvalidOperationException($"Inventory Error: Item '{line.StockCode}{line.ItemCode}' under basis '{line.StockCode}' does not exist in the stock master file.");
//                        }

//                        // Calculate net available inventory in the database record unit format
//                        decimal requestedInStockUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.Quantity);
//                        decimal netAvailableInStockUnit = stockRecord.QtyInHand - stockRecord.ShadowBalance - stockRecord.SrnBalance;

//                        if (requestedInStockUnit > netAvailableInStockUnit)
//                        {
//                            throw new InvalidOperationException($"Deficit Block: Attempted to exceed balance quantity for Item '{line.StockCode}{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Available: {await _unitConversionService.ConvertUnitAsync(stockRecord.Unit, line.Unit, netAvailableInStockUnit)} {line.Unit}.");
//                        }

//                        // 3. WRITE TO THE TRANSACTION LEDGER RECORD TABLE (OrderwiseStockTransactions)
//                        var trxLine = new OrderwiseStockTransaction
//                        {
//                            DocumentNumber = header.SrnNumber,
//                            TransactionType = "0S", // '0S' = Stores Requisition Note
//                            TransactionDate = header.TransactionDate,
//                            BuyerCode = header.BuyerCode,
//                            Order = header.Order,
//                            DepartmentCode = header.DepartmentCode,
//                            StockCode = line.StockCode,
//                            ItemCode = line.ItemCode,
//                            Unit = line.Unit,
//                            Quantity = line.Quantity,
//                            CreatedByUsername = username.Trim().ToUpper()
//                        };
//                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(trxLine);

//                        // 4. UPDATE MASTER ITEM CATALOG VALUES IN THE CORRECT TABLE (OrderwiseStockMasters)
//                        var catalogItem = await _apparelProDbContext.OrderwiseStockMasters
//                            .FirstOrDefaultAsync(m => m.BuyerCode == header.BuyerCode && m.Order == header.Order && m.st == line.StockCode && m.ItemCode == line.ItemCode);

//                        if (catalogItem != null)
//                        {
//                            decimal requestedInCatalogUnit = await _unitConversionService.ConvertUnitAsync(line.Unit, catalogItem.UnitCode ?? "PCS", line.Quantity);

//                            // Increment your target catalog requisitioned quantity metric column safely
//                            catalogItem.RequisitionedQuantity += requestedInCatalogUnit;
//                            _apparelProDbContext.OrderwiseStockMasters.Update(catalogItem);
//                        }

//                        // 5. UPDATE LOCK WITHIN THE MASTER LEDGER POOL BALANCE (OrderwiseStocks)
//                        stockRecord.SrnBalance += requestedInStockUnit; // Locks the allocation balance!
//                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);
//                    }

//                    await _apparelProDbContext.SaveChangesAsync();
//                    await dbTransaction.CommitAsync();
//                    return true;
//                }
//                catch (Exception)
//                {
//                    await dbTransaction.RollbackAsync();
//                    throw;
//                }
//            }
//        }

//        // ----------------------------------------------------------------------------------
//        // 🔒 THREAD-SAFE CONSECUTIVE DOCUMENT NUMBER GENERATION ENGINE
//        // Safely increments the counter and pads with leading zeros to maintain serial continuity
//        // ----------------------------------------------------------------------------------
//        private async Task<string> GenerateNextDocumentNumberAsync(string noteType)
//        {
//            noteType = noteType.Trim().ToUpper();

//            var sequence = await _apparelProDbContext.DocumentSequences
//                .FirstOrDefaultAsync(s => s.NoteType == noteType);

//            if (sequence == null)
//            {
//                throw new InvalidOperationException($"Sequence Error: Document counter configuration for note type '{noteType}' was not found.");
//            }

//            // Increment the counter tracking value atomically
//            sequence.LastAllocatedNumber += 1;
//            _apparelProDbContext.DocumentSequences.Update(sequence);
//            await _apparelProDbContext.SaveChangesAsync();

//            // Pads with leading zeros to create consistent 6-character strings ("000001", "000002", etc.)
//            string formattedNumber = sequence.LastAllocatedNumber.ToString().PadLeft(6, '0');
//            return $"{sequence.Prefix}{formattedNumber}";
//        }
//    }
//}
