using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.GeneralInventory;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_OGTN1.PRG/GI_OGTN2.PRG - the General Inventory <->
    // Orderwise Inventory transfer bridge ("GOODS TRANSFER NOTE (Orders)"). Moves stock
    // between a General store and a specific Buyer/Order's stock. Deliberate deviations
    // from legacy (all confirmed against the actual .PRG source, not assumed):
    //   1. One document number (NoteType "OGTN"), not legacy's two synced sequences
    //      ("General GTN No" + "Order-wise GTN No") - written to both ledgers.
    //   2. No live ShadowBalance reservation during entry - atomic validate-then-commit,
    //      same convention GoodsTransferNoteService (pure Orderwise GTN) already
    //      established for this exact reason.
    //   3. Order-side ledger rows use "6TG"/"1TG" instead of reusing pure Orderwise GTN's
    //      "6T"/"1T" - otherwise these bridge rows would be indistinguishable from an
    //      ordinary Orderwise<->Orderwise transfer in reports. General-side ledger keeps
    //      legacy's own "6TO"/"1TO" verbatim.
    //   4. The od_sacc2 (StyleMaterialCostProfile) auto-seed on the GeneralToOrder leg is
    //      dropped - that entity's real key requires TypeCode+StyleCode, which General
    //      Inventory has no way to supply. Fabricating them would violate the
    //      zero-assumption rule; this is a cosmetic legacy side-effect only (doesn't
    //      affect any stock quantity).
    //   5. No gi_monst writes - same "aggregate live" precedent as every other General
    //      Inventory note type in this codebase.
    public class OrderGoodsTransferService : IOrderGoodsTransferService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public OrderGoodsTransferService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<List<OrderGtnTransferableStockRowServiceModel>> GetTransferableStockAsync(
            string direction, string storeCode, int buyerCode, string order)
        {
            storeCode = storeCode.Trim().ToUpper();
            order = order.Trim();

            var storeExists = await _apparelProDbContext.GeneralStores.AsNoTracking().AnyAsync(s => s.Code == storeCode);
            if (!storeExists)
                throw new KeyNotFoundException($"Invalid Stores Code '{storeCode}'.");

            var buyerOrderExists = await _apparelProDbContext.PurchaseOrders.AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!buyerOrderExists)
                throw new KeyNotFoundException($"Invalid Buyer/Order (Buyer: {buyerCode}, Order: {order}).");

            List<(string ItemCode, string Unit, decimal AvailableBalance)> rows;

            if (direction == OrderGtnDirection.GeneralToOrder)
            {
                var generalRows = await _apparelProDbContext.GeneralStockMasters
                    .AsNoTracking()
                    .Where(m => m.StoreCode == storeCode && (m.QtyInHand - m.ShadowBalance) > 0)
                    .Select(m => new { m.ItemCode, m.Unit, Available = m.QtyInHand - m.ShadowBalance })
                    .ToListAsync();
                rows = generalRows.Select(r => (r.ItemCode, r.Unit, r.Available)).ToList();
            }
            else if (direction == OrderGtnDirection.OrderToGeneral)
            {
                var orderRows = await _apparelProDbContext.OrderwiseStocks
                    .AsNoTracking()
                    .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.StoreCode == storeCode
                        && (s.QtyInHand - s.ShadowBalance) > 0)
                    .Select(s => new { s.ItemCode, s.Unit, Available = s.QtyInHand - s.ShadowBalance })
                    .ToListAsync();
                rows = orderRows.Select(r => (r.ItemCode, r.Unit, r.Available)).ToList();
            }
            else
            {
                throw new ArgumentException($"Invalid Direction '{direction}'.");
            }

            if (rows.Count == 0)
                return new List<OrderGtnTransferableStockRowServiceModel>();

            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return rows.Select(r => new OrderGtnTransferableStockRowServiceModel
            {
                StoreCode = storeCode,
                ItemCode = r.ItemCode,
                Description = descriptions.GetValueOrDefault(r.ItemCode, "(No description available)"),
                Unit = r.Unit,
                AvailableBalance = r.AvailableBalance,
            }).ToList();
        }

        public async Task<bool> CommitOrderGoodsTransferNoteAsync(
            OrderGtnHeaderServiceModel header,
            List<OrderGtnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("Transaction Aborted: Goods Transfer Note (Orders) cannot be committed without line items.");

            if (header.Direction != OrderGtnDirection.GeneralToOrder && header.Direction != OrderGtnDirection.OrderToGeneral)
                throw new ArgumentException($"Invalid Direction '{header.Direction}'.");

            header.Order = header.Order.Trim();
            bool isGeneralToOrder = header.Direction == OrderGtnDirection.GeneralToOrder;

            var buyerOrderExists = await _apparelProDbContext.PurchaseOrders.AsNoTracking()
                .AnyAsync(po => po.BuyerCode == header.BuyerCode && po.Order == header.Order);
            if (!buyerOrderExists)
                throw new InvalidOperationException($"Invalid Buyer/Order (Buyer: {header.BuyerCode}, Order: {header.Order}).");

            using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync();
            try
            {
                string allocatedOgtnNumber = await _sharedService.GenerateNextDocumentNumberAsync("OGTN");

                var generalMasters = new Dictionary<string, GeneralStockMaster>();
                var orderwiseStocks = new Dictionary<string, OrderwiseStock>();

                // Pass 1: lock and validate every line before writing anything.
                foreach (var line in lines)
                {
                    line.StoreCode = line.StoreCode.Trim().ToUpper();
                    line.ItemCode = line.ItemCode.Trim();
                    line.Unit = line.Unit.Trim().ToUpper();
                    string lineKey = line.StoreCode + "|" + line.ItemCode;

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                    var generalMaster = await _apparelProDbContext.GeneralStockMasters
                        .FromSqlInterpolated($@"SELECT * FROM GeneralStockMasters WITH (UPDLOCK, HOLDLOCK)
                            WHERE StoreCode = {line.StoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();
                    if (generalMaster == null)
                        throw new InvalidOperationException($"Item '{line.ItemCode}' not found in General Stores '{line.StoreCode}'.");
                    generalMasters[lineKey] = generalMaster;

                    var orderwiseStock = await _apparelProDbContext.OrderwiseStocks
                        .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                            WHERE BuyerCode = {header.BuyerCode} AND [Order] = {header.Order}
                              AND StoreCode = {line.StoreCode} AND ItemCode = {line.ItemCode}")
                        .FirstOrDefaultAsync();

                    if (isGeneralToOrder)
                    {
                        // Mirrors legacy: balance is checked against the General store's own
                        // balance; the destination Buyer/Order stock row is auto-created below
                        // if it doesn't exist yet (legacy's "if !found() add_rec()").
                        decimal requestedInGeneralUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);
                        if (requestedInGeneralUnit > generalMaster.QtyInHand - generalMaster.ShadowBalance)
                            throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for Item '{line.ItemCode}'.");
                    }
                    else
                    {
                        // OrderToGeneral: the Buyer/Order must already carry this item/store -
                        // mirrors legacy's hard "Item not found Buyer/Order." validation, no
                        // auto-create on this leg.
                        if (orderwiseStock == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under Store '{line.StoreCode}' not found in Buyer/Order stock.");

                        decimal requestedInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, orderwiseStock.Unit, line.Quantity);
                        if (requestedInStockUnit > orderwiseStock.QtyInHand - orderwiseStock.ShadowBalance)
                            throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for Item '{line.ItemCode}'.");
                    }

                    if (orderwiseStock != null)
                        orderwiseStocks[lineKey] = orderwiseStock;
                }

                // Pass 2: write everything now that every line has passed.
                foreach (var line in lines)
                {
                    string lineKey = line.StoreCode + "|" + line.ItemCode;
                    var generalMaster = generalMasters[lineKey];
                    orderwiseStocks.TryGetValue(lineKey, out var orderwiseStock);

                    if (isGeneralToOrder)
                    {
                        decimal avgPrice = generalMaster.QtyInHand == 0 ? 0 : Math.Round(generalMaster.Value / generalMaster.QtyInHand, 2);
                        decimal qtyInGeneralUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);

                        generalMaster.QtyInHand -= qtyInGeneralUnit;
                        generalMaster.Value -= Math.Round(qtyInGeneralUnit * avgPrice, 2);
                        _apparelProDbContext.GeneralStockMasters.Update(generalMaster);

                        var orderwiseMaster = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode} AND [Order] = {header.Order} AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();
                        if (orderwiseMaster == null)
                        {
                            orderwiseMaster = new OrderwiseStockMaster
                            {
                                BuyerCode = header.BuyerCode,
                                Order = header.Order,
                                ItemCode = line.ItemCode,
                                Unit = line.Unit,
                                Currency = generalMaster.Currency,
                                Price = avgPrice,
                            };
                            await _apparelProDbContext.OrderwiseStockMasters.AddAsync(orderwiseMaster);
                        }
                        decimal qtyInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, orderwiseMaster.Unit, line.Quantity);
                        orderwiseMaster.TransferInQuantity += qtyInMasterUnit;

                        if (orderwiseStock == null)
                        {
                            orderwiseStock = new OrderwiseStock
                            {
                                BuyerCode = header.BuyerCode,
                                Order = header.Order,
                                StoreCode = line.StoreCode,
                                ItemCode = line.ItemCode,
                                Unit = line.Unit,
                            };
                            await _apparelProDbContext.OrderwiseStocks.AddAsync(orderwiseStock);
                        }
                        decimal qtyInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, orderwiseStock.Unit, line.Quantity);
                        orderwiseStock.QtyInHand += qtyInStockUnit;
                        orderwiseStock.ToDateReceived += qtyInStockUnit;
                        orderwiseStock.LastDateReceived = header.TransactionDate;

                        await _apparelProDbContext.GeneralStockTransactions.AddAsync(new GeneralStockTransaction
                        {
                            TransactionTypeCode = "6TO",
                            DocumentNumber = allocatedOgtnNumber,
                            TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                            TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            Price = avgPrice,
                            Currency = generalMaster.Currency,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                        });
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedOgtnNumber,
                            TransactionType = "1TG",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Length >= 2 ? line.ItemCode.Substring(0, 2) : line.ItemCode,
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            CreatedByUsername = username.Trim().ToUpper(),
                        });
                    }
                    else
                    {
                        // orderwiseStock is guaranteed non-null here (validated in Pass 1).
                        var stock = orderwiseStock!;
                        decimal qtyInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, stock.Unit, line.Quantity);
                        stock.QtyInHand -= qtyInStockUnit;
                        stock.ToDateReceived -= qtyInStockUnit;

                        var orderwiseMaster = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode} AND [Order] = {header.Order} AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();
                        if (orderwiseMaster != null)
                        {
                            decimal qtyInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, orderwiseMaster.Unit, line.Quantity);
                            orderwiseMaster.TransferOutQuantity += qtyInMasterUnit;
                        }

                        decimal avgPrice = generalMaster.QtyInHand == 0 ? 0 : Math.Round(generalMaster.Value / generalMaster.QtyInHand, 2);
                        decimal qtyInGeneralUnit = await _sharedService.ConvertUnitAsync(line.Unit, generalMaster.Unit, line.Quantity);
                        generalMaster.QtyInHand += qtyInGeneralUnit;
                        generalMaster.Value += Math.Round(qtyInGeneralUnit * avgPrice, 2);
                        _apparelProDbContext.GeneralStockMasters.Update(generalMaster);

                        await _apparelProDbContext.GeneralStockTransactions.AddAsync(new GeneralStockTransaction
                        {
                            TransactionTypeCode = "1TO",
                            DocumentNumber = allocatedOgtnNumber,
                            TransactionDate = DateOnly.FromDateTime(header.TransactionDate),
                            TransactionTime = TimeOnly.FromDateTime(DateTime.Now),
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            Price = avgPrice,
                            Currency = generalMaster.Currency,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                        });
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedOgtnNumber,
                            TransactionType = "6TG",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Length >= 2 ? line.ItemCode.Substring(0, 2) : line.ItemCode,
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            CreatedByUsername = username.Trim().ToUpper(),
                        });
                    }
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

        public async Task<OrderGtnPrintDetailsServiceModel> GetOrderGtnPrintDetailsAsync(string ogtnNumber)
        {
            ogtnNumber = ogtnNumber.Trim();

            var rows = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => t.DocumentNumber == ogtnNumber && (t.TransactionTypeCode == "6TO" || t.TransactionTypeCode == "1TO"))
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (rows.Count == 0)
                throw new KeyNotFoundException($"OGTN No '{ogtnNumber}' not found.");

            var first = rows[0];
            string direction = first.TransactionTypeCode == "6TO" ? OrderGtnDirection.GeneralToOrder : OrderGtnDirection.OrderToGeneral;

            var buyer = first.BuyerCode.HasValue
                ? await _apparelProDbContext.Buyers.AsNoTracking().FirstOrDefaultAsync(b => b.BuyerCode == first.BuyerCode.Value)
                : null;

            var itemCodes = rows.Select(r => r.ItemCode).Distinct().ToList();
            var descriptions = await _apparelProDbContext.GeneralStockReferences
                .AsNoTracking()
                .Where(r => itemCodes.Contains(r.ItemCode))
                .ToDictionaryAsync(r => r.ItemCode, r => r.Description);

            return new OrderGtnPrintDetailsServiceModel
            {
                Header = new OrderGtnPrintHeaderServiceModel
                {
                    OgtnNumber = ogtnNumber,
                    Direction = direction,
                    BuyerCode = first.BuyerCode ?? 0,
                    BuyerName = buyer?.Name ?? "",
                    Order = first.Order ?? "",
                    TransactionDate = first.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    PrintedOn = DateTime.Now,
                },
                Lines = rows.Select(r => new OrderGtnPrintLineServiceModel
                {
                    StoreCode = r.StoreCode,
                    ItemCode = r.ItemCode,
                    Description = descriptions.GetValueOrDefault(r.ItemCode, ""),
                    Unit = r.Unit,
                    Quantity = r.Quantity,
                }).ToList(),
            };
        }
    }
}
