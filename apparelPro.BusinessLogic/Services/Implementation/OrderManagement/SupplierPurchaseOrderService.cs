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
        private readonly IMaterialConsumptionService _materialConsumptionService;
        public SupplierPurchaseOrderService(IMapper mapper, ApparelProDbContext apparelProDbContext,
            ILookupConstants lookupConstants, IMaterialConsumptionService materialConsumptionService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _materialConsumptionService = materialConsumptionService;
        }
        public async Task<List<AvailableBudgetLineServiceModel>> GetUnfulfilledBudgetLinesAsync(int buyerCode, string order)
        {
            order = order.Trim();

            // Query active planned material lines with a strict balance restriction guard filter
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order == order && p.BalanceQuantity > 0)
                .ToListAsync();

            var resultList = new List<AvailableBudgetLineServiceModel>();

            foreach (var profile in costProfiles)
            {
                // Cross-reference user-friendly descriptions from your master checklist reference table
                var masterItem = await _apparelProDbContext.OrderItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.StockCode == profile.StockCode && i.ItemCode == profile.ItemCode);

                resultList.Add(new AvailableBudgetLineServiceModel
                {
                    ItemCode = $"{profile.StockCode}{profile.ItemCode}{profile.Feature1}{profile.Feature2}{profile.Feature3}{profile.Feature4}", // Unified 22+ Character Composite String Signature
                    ItemUnit = profile.ItemUnit,
                    BalanceQuantity = profile.BalanceQuantity,
                    TypeCode = profile.TypeCode,
                    StyleCode = profile.StyleCode,
                    Description = masterItem?.Description ?? "Allocated Raw Material Component Row"
                });
            }

            return resultList;
        }


        public async Task<bool> SaveSupplierPurchaseOrderAsync(SaveSupplierPORequestServiceModel request)
        {
            var header = request.Header;
            string poNoSanitized = header.PurchaseNumber.Trim();
            string storeCode = header.StoreCode.Trim();
            string currencyCode = header.CurrencyCode.Trim();

            try
            {
                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 1: Loop and process line item rows sequentially
                // ---------------------------------------------------------------------
                foreach (var line in request.LineItems)
                {
                    if (string.IsNullOrEmpty(line.OrderUnit) || line.OrderQuantity <= 0) continue;

                    decimal historicalQuantity = 0;
                    string historicalUnit = "";

                    var existingDetail = await _apparelProDbContext.PODetails
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

                        _apparelProDbContext.PODetails.Update(existingDetail);
                    }
                    else
                    {
                        var newDetail = new PODetails
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
                        var detailEntry = await _apparelProDbContext.PODetails.AddAsync(newDetail);
                        detailEntry.State = EntityState.Added;
                    }

                    // ---------------------------------------------------------------------
                    // TRANSACTION PHASE 2: Adjust planned Material Cost Profile balances (od_sacc2)
                    // ---------------------------------------------------------------------


                    // Slices out only the base codes for category level sorting matching your structure
                    string baseStock = line.ItemCode.Substring(0, 2);
                    string baseItem = line.ItemCode.Substring(2, 4);

                    // FIXED LOOKUP: Map the parameters straight to your clear inbound DTO feature properties!
                    // This eliminates substring character index errors permanently!
                    var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                        .FirstOrDefaultAsync(p => p.BuyerCode == header.BuyerCode &&
                                                  p.Order == header.OrderNumber.Trim() &&
                                                  p.TypeCode == header.TypeCode &&
                                                  p.StyleCode == header.StyleCode.Trim() &&
                                                  p.StockCode == baseStock &&
                                                  p.ItemCode == baseItem &&
                                                  p.Feature1 == line.Feature1.Trim() &&
                                                  p.Feature2 == line.Feature2.Trim() &&
                                                  p.Feature3 == line.Feature3.Trim() &&
                                                  p.Feature4 == line.Feature4.Trim());

                    if (costProfile != null)
                    {
                        decimal historicalBalanceRebate = 0;
                        if (!string.IsNullOrEmpty(historicalUnit) && historicalQuantity > 0)
                        {
                            historicalBalanceRebate = await _materialConsumptionService.ConvertUnitAsync(historicalUnit, costProfile.ItemUnit, historicalQuantity);
                        }

                        decimal newlyOrderedInProfileUnit = await _materialConsumptionService.ConvertUnitAsync(line.OrderUnit, costProfile.ItemUnit, line.OrderQuantity);

                        // Clipper math enforcement: 56.00 - (16.00 + 8.00) = 32.00 GRS!
                        costProfile.BalanceQuantity = (costProfile.BalanceQuantity + historicalBalanceRebate) - newlyOrderedInProfileUnit;

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
                            await _materialConsumptionService.ConvertUnitAsync(historicalUnit, stockRecord.Unit, historicalQuantity) : 0;
                        decimal newQtyInStockUnit = await _materialConsumptionService.ConvertUnitAsync(line.OrderUnit, stockRecord.Unit, line.OrderQuantity);

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
                            await _materialConsumptionService.ConvertUnitAsync(historicalUnit, stockMaster.Unit, historicalQuantity) : 0;
                        decimal newQtyInMasterUnit = await _materialConsumptionService.ConvertUnitAsync(line.OrderUnit, stockMaster.Unit, line.OrderQuantity);

                        stockMaster.OrderedQuantity = stockMaster.OrderedQuantity - oldQtyInMasterUnit + newQtyInMasterUnit;
                        stockMaster.Currency = currencyCode;
                        stockMaster.Price = line.UnitPrice;
                        _apparelProDbContext.OrderwiseStockMasters.Update(stockMaster);
                    }
                }

                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 4: Update the Master PO Header File (od_pohed) (FIXED)
                // ---------------------------------------------------------------------
                var poHeaderRow = await _apparelProDbContext.PurchaseOrderHeaders
                    .FirstOrDefaultAsync(h => h.PurchaseOrderNumber == poNoSanitized);

                if (poHeaderRow == null)
                {
                    var newHeader = new PurchaseOrderHeader
                    {
                        PurchaseOrderNumber = poNoSanitized,
                        SupplierCode = header.SupplierCode.ToString(),
                        StoreCode = storeCode,
                        ProformaInvoiceNo = header.ProformaInvoiceNo.Trim(),
                        ProformaInvoiceDate = header.ProformaInvoiceDate,
                        CurrencyCode = currencyCode,
                        IsPoUsed = false
                    };

                    // FIXED: Force EF Core to run an SQL INSERT instead of an UPDATE
                    var headerEntry = await _apparelProDbContext.PurchaseOrderHeaders.AddAsync(newHeader);
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

                    _apparelProDbContext.PurchaseOrderHeaders.Update(poHeaderRow);
                }

                // 3. ATOMIC ENFORCEMENT: A single SaveChangesAsync call processes all track adjustments cleanly
                await _apparelProDbContext.SaveChangesAsync(); return true;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> SaveSupplierPurchaseOrderAsync1(SaveSupplierPORequestServiceModel request)
        {
            var header = request.Header;
            string poNoSanitized = header.PurchaseNumber.Trim();
            string storeCode = header.StoreCode.Trim();
            string currencyCode = header.CurrencyCode.Trim();

            try
            {
                // PHASE 1: Process and insert each staged procurement line item dynamically
                foreach (var line in request.LineItems)
                {
                    var existingDetail = await _apparelProDbContext.PODetails
                        .FirstOrDefaultAsync(d => d.PONumber == poNoSanitized &&
                                                  d.Buyer == header.BuyerCode &&
                                                  d.Order == header.OrderNumber.Trim() &&
                                                  d.ItemCode == line.ItemCode.Trim());

                    decimal historicalQuantity = 0;
                    string historicalUnit = "";

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

                        _apparelProDbContext.PODetails.Update(existingDetail);
                    }
                    else
                    {
                        //var newDetail = new PODetails
                        //{
                        //    PONumber = poNoSanitized,
                        //    Buyer = header.BuyerCode,
                        //    Order = header.OrderNumber.Trim(),
                        //    Type = header.TypeCode,
                        //    Style = header.StyleCode.Trim(),
                        //    ItemCode = line.ItemCode.Trim(),
                        //    RefNo = line.RefNo?.Trim(),
                        //    OrderUnit = line.OrderUnit.Trim(),
                        //    OrderQuantity = line.OrderQuantity,
                        //    UnitPrice = line.UnitPrice,
                        //    ExportDate = line.ExportDate,
                        //    LCNo = line.LcNo?.Trim(),
                        //    Balance = line.OrderQuantity // Initial open delivery balance equals ordered qty
                        //};
                        //var entry = await _apparelProDbContext.PODetails.AddAsync(newDetail);
                        //entry.State = EntityState.Added;

                        // FORCE INSERT STATE: This explicitly breaks EF Core out of its tracking guess loops,
                        // instructing the engine to map this row strictly using an SQL INSERT statement.
                        //_apparelProDbContext.Entry(newDetail).State = EntityState.Added;

                        // FIXED: Execute a raw parameterized SQL statement directly to force a clean database row insertion.
                        // This bypasses EF Core's tracking snapshots completely, preventing the 0 rows affected concurrency crash!
                        await _apparelProDbContext.Database.ExecuteSqlRawAsync(@"
                           INSERT INTO dbo.PODetails (
                               PONo, Buyer, [Order], Type, Style, ItemCode, 
                               RefNo, OrderUnit, OrderQuantity, UnitPrice, ExportDate, LCNo, Balance
                           ) VALUES (
                               {0}, {1}, {2}, {3}, {4}, {5}, 
                               {6}, {7}, {8}, {9}, {10}, {11}, {12}
                           );",
                            poNoSanitized,               // {0}
                            header.BuyerCode,            // {1}
                            header.OrderNumber.Trim(),    // {2}
                            header.TypeCode,             // {3}
                            header.StyleCode.Trim(),     // {4}
                            line.ItemCode.Trim(),        // {5}
                            line.RefNo?.Trim(),          // {6}
                            line.OrderUnit.Trim(),       // {7}
                            line.OrderQuantity,          // {8}
                            line.UnitPrice,              // {9}
                            line.ExportDate,             // {10}
                            line.LcNo?.Trim(),           // {11}
                            line.OrderQuantity           // {12} - Initial delivery Balance equals OrderQuantity
                        );
                    }

                    // PHASE 2: Adjust planned Material Cost Profile balances (od_sacc2)
                    // Extract original codes back out of the incoming compound 22-char tracking key string
                    string baseStock = line.ItemCode.Substring(0, 2);
                    string baseItem = line.ItemCode.Substring(2, 4);

                    var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                        .FirstOrDefaultAsync(p => p.BuyerCode == header.BuyerCode && p.Order == header.OrderNumber.Trim() &&
                                                  p.StockCode == baseStock && p.ItemCode == baseItem);

                    if (costProfile != null)
                    {
                        decimal historicalBalanceRebate = 0;
                        if (!string.IsNullOrEmpty(historicalUnit) && historicalQuantity > 0)
                        {
                            historicalBalanceRebate = await _materialConsumptionService.ConvertUnitAsync(historicalUnit, costProfile.ItemUnit, historicalQuantity);
                        }

                        decimal newlyOrderedInProfileUnit = await _materialConsumptionService.ConvertUnitAsync(line.OrderUnit, costProfile.ItemUnit, line.OrderQuantity);

                        // Clipper formula logic: (bal_qty + old_qty) - new_qty
                        costProfile.BalanceQuantity = (costProfile.BalanceQuantity + historicalBalanceRebate) - newlyOrderedInProfileUnit;
                        _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
                    }

                    // PHASE 3: Scale Order-Wise Stocks and Stock Masters Ledger Tables
                    var stockRecord = await _apparelProDbContext.OrderwiseStocks
                        .FirstOrDefaultAsync(s => s.BuyerCode == header.BuyerCode && s.Order == header.OrderNumber.Trim() &&
                                                  s.StoreCode == storeCode && s.ItemCode == line.ItemCode.Trim());

                    if (stockRecord == null)
                    {
                        await _apparelProDbContext.OrderwiseStocks.AddAsync(new OrderwiseStock
                        {
                            BuyerCode = header.BuyerCode,
                            Order = header.OrderNumber.Trim(),
                            StoreCode = storeCode,
                            ItemCode = line.ItemCode.Trim(),
                            Unit = line.OrderUnit.Trim(),
                            OrderedQuantity = line.OrderQuantity
                        });
                    }
                    else
                    {
                        decimal oldQtyInStockUnit = !string.IsNullOrEmpty(historicalUnit) ? await _materialConsumptionService.ConvertUnitAsync(historicalUnit, stockRecord.Unit, historicalQuantity) : 0;
                        decimal newQtyInStockUnit = await _materialConsumptionService.ConvertUnitAsync(line.OrderUnit, stockRecord.Unit, line.OrderQuantity);

                        stockRecord.OrderedQuantity = stockRecord.OrderedQuantity - oldQtyInStockUnit + newQtyInStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);
                    }

                    var stockMaster = await _apparelProDbContext.OrderwiseStockMasters
                        .FirstOrDefaultAsync(m => m.BuyerCode == header.BuyerCode && m.Order == header.OrderNumber.Trim() && m.ItemCode == line.ItemCode.Trim());

                    if (stockMaster == null)
                    {
                        await _apparelProDbContext.OrderwiseStockMasters.AddAsync(new OrderwiseStockMaster
                        {
                            BuyerCode = header.BuyerCode,
                            Order = header.OrderNumber.Trim(),
                            ItemCode = line.ItemCode.Trim(),
                            Unit = line.OrderUnit.Trim(),
                            Currency = currencyCode,
                            OrderedQuantity = line.OrderQuantity,
                            Price = line.UnitPrice
                        });
                    }
                    else
                    {
                        decimal oldQtyInMasterUnit = !string.IsNullOrEmpty(historicalUnit) ?
                            await _materialConsumptionService.ConvertUnitAsync(historicalUnit, stockMaster.Unit, historicalQuantity) : 0;
                        decimal newQtyInMasterUnit = await _materialConsumptionService.ConvertUnitAsync(line.OrderUnit, stockMaster.Unit, line.OrderQuantity);

                        stockMaster.OrderedQuantity = stockMaster.OrderedQuantity - oldQtyInMasterUnit + newQtyInMasterUnit;
                        stockMaster.Currency = currencyCode;
                        stockMaster.Price = line.UnitPrice;
                        _apparelProDbContext.OrderwiseStockMasters.Update(stockMaster);
                    }
                }

                // PHASE 4: Update the Master PO Header File (od_pohed)
                var poHeaderRow = await _apparelProDbContext.PurchaseOrderHeaders.FirstOrDefaultAsync(h => h.PurchaseOrderNumber == poNoSanitized);
                if (poHeaderRow == null)
                {
                    poHeaderRow = new PurchaseOrderHeader { PurchaseOrderNumber = poNoSanitized };
                    await _apparelProDbContext.PurchaseOrderHeaders.AddAsync(poHeaderRow);
                }

                poHeaderRow.SupplierCode = header.SupplierCode.ToString();
                poHeaderRow.StoreCode = storeCode;
                poHeaderRow.ProformaInvoiceNo = header.ProformaInvoiceNo.Trim();
                poHeaderRow.ProformaInvoiceDate = header.ProformaInvoiceDate;
                poHeaderRow.CurrencyCode = currencyCode;
                poHeaderRow.IsPoUsed = false; // Release concurrent multi-user locks smoothly

                _apparelProDbContext.PurchaseOrderHeaders.Update(poHeaderRow);

                await _apparelProDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
