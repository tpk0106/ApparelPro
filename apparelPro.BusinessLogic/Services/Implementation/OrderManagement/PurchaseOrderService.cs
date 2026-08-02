using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.OrderwiseInventory;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;


namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly ISharedService _sharedService;
        public PurchaseOrderService(IMapper mapper, ApparelProDbContext apparelProDbContext,
            ILookupConstants lookupConstants, ISharedService sharedService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _sharedService = sharedService;
        }

        public async Task<PurchaseOrderServiceModel> AddPurchaseOrderAsync(CreatePurchaseOrderServiceModel createPOServiceModel)
        {
            var PODbModel = _mapper.Map<PurchaseOrder>(createPOServiceModel);
            _apparelProDbContext.PurchaseOrders.Add(PODbModel);

            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<PurchaseOrderServiceModel>(PODbModel);
        }

        public Task DeletePurchaseOrderAsync(string code)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginationResult<PurchaseOrderServiceModel>> GetPurchaseOrderAsync(int pageSize,
            int pageNumber, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            //IQueryable<PurchaseOrder> poPagination = _apparelProDbContext.PurchaseOrders.AsNoTracking();                
            // IQueryable<PurchaseOrder> poPagination = 

            var joined = _apparelProDbContext.PurchaseOrders
            .AsNoTracking()
            .Join(_apparelProDbContext.GarmentTypes, po => po.GarmentType, gt => gt.Id, (po, gt) =>
                new { po, gt })
            .AsNoTracking()
            .Join(_apparelProDbContext.Buyers, (poAndgt) => poAndgt.po.BuyerCode, buyer => buyer.BuyerCode, (po, buyer) =>
                 new PurchaseOrder
                 {
                     BasisCode = po.po.BasisCode,
                     BasisValue = po.po.BasisValue,
                     BuyerCode = po.po.BuyerCode,
                     Buyer = buyer.Name,
                     CountryCode = po.po.CountryCode,
                     CurrencyCode = po.po.CurrencyCode,
                     GarmentType = po.po.GarmentType,
                     GarmentTypeName = po.gt.TypeName,
                     Order = po.po.Order,
                     OrderDate = po.po.OrderDate,
                     Season = po.po.Season,
                     TotalQuantity = po.po.TotalQuantity,
                     UnitCode = po.po.UnitCode
                 })
            .AsNoTracking();

            //var joined1 = _apparelProDbContext.PurchaseOrders
            //.AsNoTracking()
            //.Join(_apparelProDbContext.GarmentTypes, po => po.GarmentType, gt => gt.Id, (po, gt) =>
            //new PurchaseOrder
            //{
            //    BasisCode = po.BasisCode,
            //    BasisValue = po.BasisValue,
            //    BuyerCode = po.BuyerCode,               
            //    CountryCode = po.CountryCode,
            //    CurrencyCode = po.CurrencyCode,
            //    GarmentType = po.GarmentType,
            //    GarmentTypeName = gt.TypeName,
            //    Order = po.Order,
            //    OrderDate = po.OrderDate,
            //    Season = po.Season,
            //    TotalQuantity = po.TotalQuantity,
            //    UnitCode = po.UnitCode
            //});

            IQueryable<PurchaseOrder> poPagination = (IQueryable<PurchaseOrder>)joined.AsQueryable();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(PurchaseOrder));
                poPagination = poPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }
            int counter = 0;
            counter = await poPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                poPagination = poPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            poPagination = poPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            //poPagination.Join(_apparelProDbContext.GarmentTypes,po => po.GarmentType,gt=>gt.Id,(po=>po,gt=>gt));               

            var filteredDbPos = await poPagination.ToListAsync();
            var poServiceModels = _mapper.Map<IList<PurchaseOrderServiceModel>>(filteredDbPos);

            return new PaginationResult<PurchaseOrderServiceModel>(pageSize, pageNumber, counter, poServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<PurchaseOrderServiceModel> GetPurchaseOrderByBuyerAndOrderAsync(int buyer, string order)
        {
            var PODbModel = await _apparelProDbContext.PurchaseOrders
                .Join(_apparelProDbContext.Buyers,
                    order => order.BuyerCode,
                    buyer => buyer.BuyerCode,
                    (order, buyer) => new { order, buyer })
                .Where(joinedBuyerOrder => joinedBuyerOrder.order.BuyerCode == buyer && joinedBuyerOrder.order.Order == order)
                .Select(joinedResult =>
                    new PurchaseOrder
                    {
                        BasisCode = joinedResult.order.BasisCode,
                        Buyer = joinedResult.buyer.Name,
                        Order = joinedResult.order.Order,
                        GarmentType = joinedResult.order.GarmentType,
                        CountryCode = joinedResult.order.CountryCode,

                        BuyerCode = joinedResult.buyer.BuyerCode,
                        CurrencyCode = joinedResult.order.CurrencyCode,
                        BasisValue = joinedResult.order.BasisValue,
                        OrderDate = joinedResult.order.OrderDate,
                        Season = joinedResult.order.Season,
                        TotalQuantity = joinedResult.order.TotalQuantity,
                        UnitCode = joinedResult.order.UnitCode
                    })
                .AsNoTracking()
                .FirstOrDefaultAsync();
            var POServiceModel = _mapper.Map<PurchaseOrderServiceModel>(PODbModel);
            return POServiceModel;
        }

        public async Task<List<string>> GetPurchaseOrderByBuyerCodeAsync(int buyer)
        {
            var poDbModelList = await _apparelProDbContext.PurchaseOrders
                .Where(po => po.BuyerCode == buyer && po.Order != null)
                .AsNoTracking()
                .Select(po => po.Order)
                .Distinct()
                .ToListAsync();

            return poDbModelList.ToList();
        }

        public async Task<List<StyleDetailsServiceModel>> GetStylesByBuyerCodeAndOrderCodeAndTypeCodeAsync(int buyer, string order, int type)
        {
            var poDbModelList = await _apparelProDbContext.Styles
              .Where(style => style.BuyerCode == buyer && style.Order == order && style.TypeCode == type)
              .AsNoTracking()
              //.Select(style => new { style.Buyer,style.Order, style.TypeCode,style.StyleCode })
              //.Distinct()
              .ToListAsync();
            var styleDetailsServiceModels = _mapper.Map<List<StyleDetailsServiceModel>>(poDbModelList);

            return styleDetailsServiceModels;
        }

        public async Task<List<GarmentTypeServiceModel>> GetTypesByBuyerCodeAndOrderAsync(int buyer, string order)
        {
            var typeDbModelList = await _apparelProDbContext.GarmentTypes
              .AsNoTracking()
              .Distinct()
              .ToListAsync();

            var garmentTypeServiceModels = _mapper.Map<List<GarmentTypeServiceModel>>(typeDbModelList);

            return garmentTypeServiceModels;
        }

        public Task UpdatePurchaseOrderAsync(UpdatePurchaseOrderServiceModel updateCurrencyServiceModel)
        {
            throw new NotImplementedException();
        }

        // PO for supplier

        public async Task<bool> SaveSupplierPurchaseOrderAsync(
            string purchaseNumber, string supplierCode, string storeCode,
            string proformaNo, DateOnly? proformaDate, string currencyCode,
            List<PODetails> lineItems)
        {
            // Enforce clean layout data trimming
            purchaseNumber = purchaseNumber.Trim();
            supplierCode = supplierCode.Trim();
            storeCode = storeCode.Trim();
            currencyCode = currencyCode.Trim();

            try
            {
                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 1: Process and Loop line items sequentially
                // ---------------------------------------------------------------------
                foreach (var item in lineItems)
                {
                    if (string.IsNullOrEmpty(item.OrderUnit) || item.OrderQuantity <= 0) continue;

                    decimal historicalQuantity = 0;
                    string historicalUnit = "";

                    // Check if this material allocation record line already exists under this active PO scope
                    var existingDetail = await _apparelProDbContext.PODetails
                        .FirstOrDefaultAsync(d => d.PONumber == purchaseNumber &&
                                                  d.Buyer == item.Buyer &&
                                                  d.Order == item.Order.Trim() &&
                                                  d.Type == item.Type &&
                                                  d.Style == item.Style.Trim() &&
                                                  d.ItemCode == item.ItemCode.Trim());

                    if (existingDetail != null)
                    {
                        // Capture old values for balancing equations
                        historicalQuantity = existingDetail.OrderQuantity;
                        historicalUnit = existingDetail.OrderUnit;

                        existingDetail.RefNo = item.RefNo?.Trim();
                        existingDetail.OrderUnit = item.OrderUnit.Trim();
                        existingDetail.OrderQuantity = item.OrderQuantity;
                        existingDetail.UnitPrice = item.UnitPrice;
                        existingDetail.ExportDate = item.ExportDate;

                        _apparelProDbContext.PODetails.Update(existingDetail);
                    }
                    else
                    {
                        var newDetail = new PODetails
                        {
                            PONumber = purchaseNumber,
                            Buyer = item.Buyer,
                            Order = item.Order.Trim(),
                            Type = item.Type,
                            Style = item.Style.Trim(),
                            ItemCode = item.ItemCode.Trim(),
                            RefNo = item.RefNo?.Trim(),
                            OrderUnit = item.OrderUnit.Trim(),
                            OrderQuantity = item.OrderQuantity,
                            UnitPrice = item.UnitPrice,
                            Balance = item.OrderQuantity, // Initial open delivery balance equals ordered qty
                            ExportDate = item.ExportDate
                        };
                        await _apparelProDbContext.PODetails.AddAsync(newDetail);
                    }

                    // ---------------------------------------------------------------------
                    // TRANSACTION PHASE 2: Re-balance Material Consumption Profiles (od_sacc2)
                    // ---------------------------------------------------------------------
                    var costProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                        .FirstOrDefaultAsync(p => p.BuyerCode == item.Buyer &&
                                                  p.Order == item.Order.Trim() &&
                                                  p.TypeCode == item.Type &&
                                                  p.StyleCode == item.Style.Trim() &&
                                                  p.ItemCode == item.ItemCode.Trim());

                    if (costProfile != null)
                    {
                        decimal historicalBalanceRebate = 0;
                        if (!string.IsNullOrEmpty(historicalUnit) && historicalQuantity > 0)
                        {
                            // Use your custom unit conversion method to align units with the cost profile
                            historicalBalanceRebate = await _sharedService.ConvertUnitAsync(historicalUnit, costProfile.ItemUnit, historicalQuantity);
                        }

                        decimal newlyOrderedQuantityInProfileUnit = await _sharedService
                            .ConvertUnitAsync(item.OrderUnit, costProfile.ItemUnit, item.OrderQuantity);

                        // Clipper formula alignment: (bal_qty + old_qty) - new_qty
                        costProfile.BalanceQuantity = (costProfile.BalanceQuantity + historicalBalanceRebate) - newlyOrderedQuantityInProfileUnit;

                        _apparelProDbContext.StyleMaterialCostProfiles.Update(costProfile);
                    }

                    // ---------------------------------------------------------------------
                    // TRANSACTION PHASE 3: Scale Store Inventories (in_stock & in_stmst)
                    // ---------------------------------------------------------------------
                    var stockRecord = await _apparelProDbContext.OrderwiseStocks
                        .FirstOrDefaultAsync(s => s.BuyerCode == item.Buyer && s.Order == item.Order.Trim() &&
                                                  s.StoreCode == storeCode && s.ItemCode == item.ItemCode.Trim());

                    if (stockRecord == null)
                    {
                        await _apparelProDbContext.OrderwiseStocks.AddAsync(new OrderwiseStock
                        {
                            BuyerCode = item.Buyer,
                            Order = item.Order.Trim(),
                            StoreCode = storeCode,
                            ItemCode = item.ItemCode.Trim(),
                            Unit = item.OrderUnit.Trim(),
                            OrderedQuantity = item.OrderQuantity
                        });
                    }
                    else
                    {
                        decimal oldQtyInStockUnit = !string.IsNullOrEmpty(historicalUnit) ?
                            await _sharedService.ConvertUnitAsync(historicalUnit, stockRecord.Unit, historicalQuantity) : 0;
                        decimal newQtyInStockUnit = await _sharedService.ConvertUnitAsync(item.OrderUnit, stockRecord.Unit, item.OrderQuantity);

                        stockRecord.OrderedQuantity = stockRecord.OrderedQuantity - oldQtyInStockUnit + newQtyInStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);
                    }

                    var stockMaster = await _apparelProDbContext.OrderwiseStockMasters
                        .FirstOrDefaultAsync(m => m.BuyerCode == item.Buyer && m.Order == item.Order.Trim() && 
                        m.ItemCode == item.ItemCode.Trim());

                    if (stockMaster == null)
                    {
                        await _apparelProDbContext.OrderwiseStockMasters.AddAsync(new OrderwiseStockMaster
                        {
                            BuyerCode = item.Buyer,
                            Order = item.Order.Trim(),
                            ItemCode = item.ItemCode.Trim(),
                            Unit = item.OrderUnit.Trim(),
                            Currency = currencyCode,
                            OrderedQuantity = item.OrderQuantity,
                            Price = item.UnitPrice
                        });
                    }
                    else
                    {
                        decimal oldQtyInMasterUnit = !string.IsNullOrEmpty(historicalUnit) ?
                            await _sharedService.ConvertUnitAsync(historicalUnit, stockMaster.Unit, historicalQuantity) : 0;
                        decimal newQtyInMasterUnit = await _sharedService.ConvertUnitAsync(item.OrderUnit, stockMaster.Unit, item.OrderQuantity);

                        stockMaster.OrderedQuantity = stockMaster.OrderedQuantity - oldQtyInMasterUnit + newQtyInMasterUnit;
                        stockMaster.Currency = currencyCode;
                        stockMaster.Price = item.UnitPrice;
                        _apparelProDbContext.OrderwiseStockMasters.Update(stockMaster);
                    }
                }

                // ---------------------------------------------------------------------
                // TRANSACTION PHASE 4: Commit and finalize the PO Header Record (od_pohed)
                // ---------------------------------------------------------------------
                var header = await _apparelProDbContext.PurchaseOrderHeaders
                    .FirstOrDefaultAsync(h => h.PurchaseOrderNumber == purchaseNumber);
                if (header == null)
                {
                    header = new PurchaseOrderHeader { PurchaseOrderNumber = purchaseNumber };
                    await _apparelProDbContext.PurchaseOrderHeaders.AddAsync(header);
                }

                header.SupplierCode = supplierCode;
                header.StoreCode = storeCode;
                header.ProformaInvoiceNo = proformaNo.Trim();
                header.ProformaInvoiceDate = proformaDate;
                header.CurrencyCode = currencyCode;
                header.IsPoUsed = false; // Release multi-user record edit lock cleanly upon successful completion

                _apparelProDbContext.PurchaseOrderHeaders.Update(header);

                // Atomic commit saves all modifications inside an implicit transaction block safely supporting MARS connections
                await _apparelProDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Automatically rolls back any pending model adjustments on error
                throw;
            }
        }
    
    }
}
