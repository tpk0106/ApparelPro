using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.OrderwiseInventory;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class SupplierPurchaseOrderService : ISupplierPurchaseOrderService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly ISharedService _sharedService;
        private readonly IStyleApprovalService _styleApprovalService;
        public SupplierPurchaseOrderService(IMapper mapper, ApparelProDbContext apparelProDbContext,
            ILookupConstants lookupConstants,
            ISharedService sharedService,
            IStyleApprovalService styleApprovalService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _sharedService = sharedService;
            _styleApprovalService = styleApprovalService;
        }
        public async Task<List<AvailableBudgetLineServiceModel>> GetUnfulfilledBudgetLinesAsync(int buyerCode, string order)
        {
            order = order.Trim();

            // Query active planned material lines with a strict balance restriction guard filter
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order == order && p.BalanceQuantity > 0)
                .ToListAsync();

            // Bulk-fetch Trim Sheet Approval status for every distinct Style referenced above,
            // instead of one IStyleApprovalService call per line - purely informational here
            // (lets the picker UI show/grey out lines proactively); the real enforced gate is
            // the hard check inside SaveSupplierPurchaseOrderAsync below, same "client-facing
            // flag + server-side guard" split already used for the budget-deficit check in
            // this file. Mirrors GetStyleApprovalDetailsAsync's own "is this locked" condition.
            var approvedStyleKeys = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order &&
                            s.ApprovedDate.HasValue && s.ApprovedDate != DateOnly.FromDateTime(DateTime.MinValue))
                .Select(s => new { s.TypeCode, s.StyleCode })
                .ToListAsync();

            var approvedStyleSet = approvedStyleKeys
                .Select(s => (s.TypeCode, s.StyleCode))
                .ToHashSet();

            // "Main Material" needs the SPECIFIC material type (BUTTON/FABRIC/ZIP), not the
            // broad Stock category (e.g. "Raw material"/"Accessories" - confirmed too coarse by
            // the user 2026-08-03). That specific granularity lives in the StockItems catalog
            // (not OrderItems - see MaterialConsumptionService.GetMaterialCatalogAsync for why
            // StockItems, not OrderItems, is the live catalog every screen now standardizes on),
            // keyed by (StockCode, ItemCode) - the same table MaterialConsumptionService already
            // uses for this exact purpose (GetMaterialCatalogAsync/GetLedgerEntriesByStyleAsync's
            // descriptionLookup). Bulk-fetch once, not per-line.
            var orderItemDescriptionLookup = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .ToDictionaryAsync(i => (i.StockCode, i.ItemCode), i => i.Description);

            // Kept as a fallback only (when an item has no catalog entry) - the broad
            // Stock category is still better than showing nothing.
            var stockDescriptionLookup = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .ToDictionaryAsync(s => s.StockCode, s => s.Description);

            var resultList = new List<AvailableBudgetLineServiceModel>();

            foreach (var profile in costProfiles)
            {
                // Composite ItemCode layout: StockCode(2) + ItemCode(4) + Feature1-4(4 each) = 22 chars
                // (ComposeCostProfileItemCode's own convention).
                // .Trim() is required here: ComposeCostProfileItemCode's Segment() helper pads each
                // fixed-width segment with TRAILING SPACES (Trim().PadRight(width)), so a raw Substring()
                // extraction retains those trailing spaces (e.g. "BT" -> "BT  ") and silently fails to match
                // the OrderItems dictionary's unpadded (StockCode, ItemCode) keys via TryGetValue - confirmed
                // by the user 2026-08-03 ("still No ItemCode (02BT) description 'BUTTON' displays").
                var stockCode = (profile.ItemCode.Length >= 2 ? profile.ItemCode.Substring(0, 2) : profile.ItemCode).Trim();
                var itemCodeSegment = (profile.ItemCode.Length >= 6 ? profile.ItemCode.Substring(2, 4) : string.Empty).Trim();

                string mainMaterialName;
                if (orderItemDescriptionLookup.TryGetValue((stockCode, itemCodeSegment), out var itemDescription) && !string.IsNullOrWhiteSpace(itemDescription))
                {
                    mainMaterialName = itemDescription;
                }
                else if (stockDescriptionLookup.TryGetValue(stockCode, out var stockDescription) && !string.IsNullOrWhiteSpace(stockDescription))
                {
                    mainMaterialName = stockDescription;
                }
                else
                {
                    mainMaterialName = stockCode;
                }

                resultList.Add(new AvailableBudgetLineServiceModel
                {
                    // StyleMaterialCostProfiles.ItemCode is already the full 22-char composite
                    // (collapsed 2026-07-22 from separate StockCode/ItemCode/Feature1-4 columns) —
                    // no more concatenation needed here.
                    ItemCode = profile.ItemCode,
                    ItemUnit = profile.ItemUnit,
                    BalanceQuantity = profile.BalanceQuantity,
                    TypeCode = profile.TypeCode,
                    StyleCode = profile.StyleCode,
                    Description = !string.IsNullOrWhiteSpace(profile.Description)
                        ? profile.Description
                        : "(No description available)",
                    IsStyleApproved = approvedStyleSet.Contains((profile.TypeCode, profile.StyleCode)),
                    MainMaterialName = mainMaterialName
                });
            }

            return resultList;
        }


        public async Task<string> SaveSupplierPurchaseOrderAsync(SaveSupplierPORequestServiceModel request)
        {
            var header = request.Header;
            string storeCode = header.StoreCode.Trim();
            string currencyCode = header.CurrencyCode.Trim();

            // TRIM SHEET APPROVAL GATE (2026-08-03): mirrors OD_ACORD.PRG's rajiva() function,
            // which checks "!empty(od_style->userid)" before letting a material-balance line be
            // pulled into a Purchase Order, else "Trim Sheet not Approved." A Supplier P/O in
            // this modern system is scoped to a single Buyer/Order/Type/Style per request
            // (POHeaderServiceModel), unlike legacy's multi-style-per-P/O loop, so this only
            // needs to run once here rather than per line item. Checked before opening the
            // transaction below - a cheap fail-fast, no need to touch the database for a request
            // we're about to reject anyway.
            var approvalDetails = await _styleApprovalService.GetStyleApprovalDetailsAsync(
                header.BuyerCode, header.OrderNumber, header.TypeCode, header.StyleCode);

            if (approvalDetails == null)
            {
                throw new InvalidOperationException(
                    $"Cannot raise a Purchase Order: the Trim Sheet for Style '{header.StyleCode}' has not been approved yet. Approve the Material Consumption sheet for this style first.");
            }

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 0: Allocate a real P/O number for new P/Os via the
                // same thread-safe shared document sequence service STRN/GIN already
                // use (.NET equivalent of legacy's autogen/autosave against 'po_no').
                // A client-supplied PurchaseNumber is never trusted for a new P/O -
                // it's only ever used to look up an EXISTING one when editing.
                // ---------------------------------------------------------------------
                string poNoSanitized;
                if (header.IsNewPurchaseOrder)
                {
                    poNoSanitized = (await _sharedService.GenerateNextDocumentNumberAsync("PO")).Trim();
                }
                else
                {
                    poNoSanitized = header.PurchaseNumber?.Trim() ?? "";
                    if (string.IsNullOrEmpty(poNoSanitized))
                        throw new InvalidOperationException("P/O Number is required when editing an existing P/O.");

                    var existingHeaderCheck = await _apparelProDbContext.SupplierPurchaseOrders
                        .AsNoTracking()
                        .FirstOrDefaultAsync(h => h.PurchaseOrderNumber == poNoSanitized);
                    if (existingHeaderCheck == null)
                        throw new InvalidOperationException($"P/O No. '{poNoSanitized}' does not exist.");
                }

                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 1: Loop and process line item rows sequentially
                // ---------------------------------------------------------------------
                foreach (var line in request.LineItems)
                {
                    if (string.IsNullOrEmpty(line.OrderUnit) || line.OrderQuantity <= 0) continue;

                    decimal historicalQuantity = 0;
                    string historicalUnit = "";

                    var existingDetail = await _apparelProDbContext.SupplierPurchaseOrderDetails
                        .FirstOrDefaultAsync(d => d.PONumber == poNoSanitized &&
                                                  d.Buyer == header.BuyerCode &&
                                                  d.Order == header.OrderNumber.Trim() &&
                                                  d.ItemCode == line.ItemCode.Trim());

                    if (existingDetail != null)
                    {
                        historicalQuantity = existingDetail.OrderQuantity;
                        historicalUnit = existingDetail.OrderUnit;

                        existingDetail.RefNo = line.RefNo?.Trim();
                        existingDetail.OrderUnit = line.OrderUnit.Trim();
                        existingDetail.OrderQuantity = line.OrderQuantity;
                        existingDetail.UnitPrice = line.UnitPrice;
                        existingDetail.ExportDate = line.ExportDate;
                        existingDetail.LCNo = line.LcNo?.Trim();
                        existingDetail.Balance = line.OrderQuantity;

                        _apparelProDbContext.SupplierPurchaseOrderDetails.Update(existingDetail);
                    }
                    else
                    {
                        var newDetail = new SupplierPurchaseOrderDetails
                        {
                            PONumber = poNoSanitized,
                            Buyer = header.BuyerCode,
                            Order = header.OrderNumber.Trim(),
                            Type = header.TypeCode,
                            Style = header.StyleCode.Trim(),
                            ItemCode = line.ItemCode.Trim(),
                            RefNo = line.RefNo?.Trim(),
                            OrderUnit = line.OrderUnit.Trim(),
                            OrderQuantity = line.OrderQuantity,
                            UnitPrice = line.UnitPrice,
                            ExportDate = line.ExportDate,
                            LCNo = line.LcNo?.Trim(),
                            Balance = line.OrderQuantity
                        };

                        // FIXED: Explicit entry tracking configuration state enforcement
                        var detailEntry = await _apparelProDbContext.SupplierPurchaseOrderDetails.AddAsync(newDetail);
                        detailEntry.State = EntityState.Added;
                    }

                    // ---------------------------------------------------------------------
                    // TRANSACTION PHASE 2: Adjust planned Material Cost Profile balances (od_sacc2)
                    // ---------------------------------------------------------------------


                    // line.ItemCode already carries the same 22-char composite that
                    // StyleMaterialCostProfiles.ItemCode now stores directly (collapsed 2026-07-22
                    // from separate StockCode/ItemCode/Feature1-4 columns) — a plain equality match,
                    // no more Substring decomposition needed. Row-locked (UPDLOCK/HOLDLOCK) for the
                    // duration of this transaction — same pattern already used by STRN/GIN/GRN — so
                    // two concurrent P/O saves against the same budget line can't both read a stale
                    // balance and both get accepted, driving it negative.
                    // FIXED (2026-08-07): raw SQL via FromSqlInterpolated bypasses EF Core's
                    // Fluent API HasColumnName translation entirely (only plain LINQ queries get
                    // that), so this WHERE clause must use the actual physical column names
                    // (Buyer/Type/Style per StyleMaterialCostProfileConfig.cs), not the C# entity
                    // property names (BuyerCode/TypeCode/StyleCode) - was throwing "Invalid column
                    // name 'BuyerCode'./'TypeCode'./'StyleCode'." on every Supplier P/O save.
                    var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                        .FromSqlInterpolated($@"SELECT * FROM StyleMaterialCostProfiles WITH (UPDLOCK, HOLDLOCK)
                            WHERE Buyer = {header.BuyerCode}
                              AND [Order] = {header.OrderNumber.Trim()}
                              AND Type = {header.TypeCode}
                              AND Style = {header.StyleCode.Trim()}
                              AND ItemCode = {line.ItemCode.Trim()}")
                        .FirstOrDefaultAsync();

                    if (costProfile != null)
                    {
                        decimal historicalBalanceRebate = 0;
                        if (!string.IsNullOrEmpty(historicalUnit) && historicalQuantity > 0)
                        {
                            historicalBalanceRebate = await _sharedService.ConvertUnitAsync(historicalUnit, costProfile.ItemUnit, historicalQuantity);
                        }

                        decimal newlyOrderedInProfileUnit = await _sharedService.ConvertUnitAsync(line.OrderUnit, costProfile.ItemUnit, line.OrderQuantity);
                        decimal availableBeforeThisLine = costProfile.BalanceQuantity + historicalBalanceRebate;

                        // Server-side enforcement mirroring the client-side alert() check — the
                        // client check is a convenience, this is the real guard. Allows ordering
                        // exactly down to a balance of 0 (using up the full remaining budget is
                        // legitimate); only genuine overages are rejected.
                        if (newlyOrderedInProfileUnit > availableBeforeThisLine)
                            throw new InvalidOperationException($"Budget Deficit: Attempted to order more than the remaining material budget for Item '{line.ItemCode}'. Requested: {line.OrderQuantity} {line.OrderUnit}, Available: {await _sharedService.ConvertUnitAsync(costProfile.ItemUnit, line.OrderUnit, availableBeforeThisLine)} {line.OrderUnit}.");

                        // Clipper math enforcement: 56.00 - (16.00 + 8.00) = 32.00 GRS!
                        costProfile.BalanceQuantity = availableBeforeThisLine - newlyOrderedInProfileUnit;

                        _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
                    }
                    //string baseStock = line.ItemCode.Substring(0, 2);
                    //string baseItem = line.ItemCode.Substring(2, 4);

                    //var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                    //    .FirstOrDefaultAsync(p => p.BuyerCode == header.BuyerCode &&
                    //                              p.Order == header.OrderNumber.Trim() &&
                    //                              p.StockCode == baseStock &&
                    //                              p.ItemCode == baseItem);

                    //if (costProfile != null)
                    //{
                    //    decimal historicalBalanceRebate = 0;
                    //    if (!string.IsNullOrEmpty(historicalUnit) && historicalQuantity > 0)
                    //    {
                    //        historicalBalanceRebate = 
                    //            await _materialConsumptionService.ConvertUnitAsync(historicalUnit, costProfile.ItemUnit, historicalQuantity);
                    //    }

                    //    decimal newlyOrderedInProfileUnit = 
                    //        await _materialConsumptionService.ConvertUnitAsync(line.OrderUnit, costProfile.ItemUnit, line.OrderQuantity);

                    //    costProfile.BalanceQuantity = (costProfile.BalanceQuantity + historicalBalanceRebate) - newlyOrderedInProfileUnit;
                    //    _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
                    //}

                    // ---------------------------------------------------------------------
                    // TRANSACTION PHASE 3: Update Order-Wise Inventory Ledger Tables (FIXED)
                    // ---------------------------------------------------------------------
                    var stockRecord = await _apparelProDbContext.OrderwiseStocks
                        .FirstOrDefaultAsync(s => s.BuyerCode == header.BuyerCode &&
                                                  s.Order == header.OrderNumber.Trim() &&
                                                  s.StoreCode == storeCode &&
                                                  s.ItemCode == line.ItemCode.Trim());

                    if (stockRecord == null)
                    {
                        var newStock = new OrderwiseStock
                        {
                            BuyerCode = header.BuyerCode,
                            Order = header.OrderNumber.Trim(),
                            StoreCode = storeCode,
                            ItemCode = line.ItemCode.Trim(),
                            Unit = line.OrderUnit.Trim(),
                            OrderedQuantity = line.OrderQuantity
                        };

                        // FIXED: Force EF Core to run an SQL INSERT instead of an UPDATE
                        var stockEntry = await _apparelProDbContext.OrderwiseStocks.AddAsync(newStock);
                        stockEntry.State = EntityState.Added;
                    }
                    else
                    {
                        decimal oldQtyInStockUnit = !string.IsNullOrEmpty(historicalUnit) ?
                            await _sharedService.ConvertUnitAsync(historicalUnit, stockRecord.Unit, historicalQuantity) : 0;
                        decimal newQtyInStockUnit = await _sharedService.ConvertUnitAsync(line.OrderUnit, stockRecord.Unit, line.OrderQuantity);

                        stockRecord.OrderedQuantity = stockRecord.OrderedQuantity - oldQtyInStockUnit + newQtyInStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);
                    }

                    var stockMaster = await _apparelProDbContext.OrderwiseStockMasters
                        .FirstOrDefaultAsync(m => m.BuyerCode == header.BuyerCode &&
                                                  m.Order == header.OrderNumber.Trim() &&
                                                  m.ItemCode == line.ItemCode.Trim());

                    if (stockMaster == null)
                    {
                        var newMaster = new OrderwiseStockMaster
                        {
                            BuyerCode = header.BuyerCode,
                            Order = header.OrderNumber.Trim(),
                            ItemCode = line.ItemCode.Trim(),
                            Unit = line.OrderUnit.Trim(),
                            Currency = currencyCode,
                            OrderedQuantity = line.OrderQuantity,
                            Price = line.UnitPrice
                        };

                        // FIXED: Force EF Core to run an SQL INSERT instead of an UPDATE
                        var masterEntry = await _apparelProDbContext.OrderwiseStockMasters.AddAsync(newMaster);
                        masterEntry.State = EntityState.Added;
                    }
                    else
                    {
                        decimal oldQtyInMasterUnit = !string.IsNullOrEmpty(historicalUnit) ?
                            await _sharedService.ConvertUnitAsync(historicalUnit, stockMaster.Unit, historicalQuantity) : 0;
                        decimal newQtyInMasterUnit = await _sharedService.ConvertUnitAsync(line.OrderUnit, stockMaster.Unit, line.OrderQuantity);

                        stockMaster.OrderedQuantity = stockMaster.OrderedQuantity - oldQtyInMasterUnit + newQtyInMasterUnit;
                        stockMaster.Currency = currencyCode;
                        stockMaster.Price = line.UnitPrice;
                        _apparelProDbContext.OrderwiseStockMasters.Update(stockMaster);
                    }
                }

                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 4: Update the Master PO Header File (od_pohed) (FIXED)
                // ---------------------------------------------------------------------
                var poHeaderRow = await _apparelProDbContext.SupplierPurchaseOrders
                    .FirstOrDefaultAsync(h => h.PurchaseOrderNumber == poNoSanitized);

                if (poHeaderRow == null)
                {
                    var newHeader = new SupplierPurchaseOrder
                    {
                        PurchaseOrderNumber = poNoSanitized,
                        CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                        CreatedTime = TimeOnly.FromDateTime(DateTime.Now),
                        SupplierCode = header.SupplierCode.ToString(),
                        StoreCode = storeCode,
                        ProformaInvoiceNo = header.ProformaInvoiceNo.Trim(),
                        ProformaInvoiceDate = header.ProformaInvoiceDate,
                        CurrencyCode = currencyCode,
                        IsPoUsed = false
                    };

                    // FIXED: Force EF Core to run an SQL INSERT instead of an UPDATE
                    var headerEntry = await _apparelProDbContext.SupplierPurchaseOrders.AddAsync(newHeader);
                    headerEntry.State = EntityState.Added;
                }
                else
                {
                    poHeaderRow.SupplierCode = header.SupplierCode.ToString();
                    poHeaderRow.StoreCode = storeCode;
                    poHeaderRow.ProformaInvoiceNo = header.ProformaInvoiceNo.Trim();
                    poHeaderRow.ProformaInvoiceDate = header.ProformaInvoiceDate;
                    poHeaderRow.CurrencyCode = currencyCode;
                    poHeaderRow.IsPoUsed = false;

                    _apparelProDbContext.SupplierPurchaseOrders.Update(poHeaderRow);
                }

                // 3. ATOMIC ENFORCEMENT: A single SaveChangesAsync call processes all track adjustments cleanly
                await _apparelProDbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return poNoSanitized;
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        // NOTE (2026-07-22): SaveSupplierPurchaseOrderAsync1 — an unreferenced duplicate draft of
        // the method above (not declared in ISupplierPurchaseOrderService, no caller anywhere in
        // the solution) — was removed here. It still matched StyleMaterialCostProfiles by the old
        // separate StockCode/ItemCode columns, which no longer exist on that entity after the
        // 2026-07-22 collapse to a single 22-char composite ItemCode; keeping it would not compile.
    }
}
