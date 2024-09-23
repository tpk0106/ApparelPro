using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
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
        public PurchaseOrderService(IMapper mapper, ApparelProDbContext apparelProDbContext, ILookupConstants lookupConstants)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;            
        }
        public Task<PurchaseOrderServiceModel> AddPurchaseOrderAsync(CreatePurchaseOrderServiceModel createCurrencyServiceModel)
        {
            throw new NotImplementedException();
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
                new {  po, gt })
            .AsNoTracking()
            .Join(_apparelProDbContext.Buyers,(poAndgt)=>poAndgt.po.BuyerCode,buyer=>buyer.BuyerCode,(po,buyer) =>
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

            var joined1 = _apparelProDbContext.PurchaseOrders
            .AsNoTracking()
            .Join(_apparelProDbContext.GarmentTypes, po => po.GarmentType, gt => gt.Id, (po, gt) =>
            new PurchaseOrder
            {
                BasisCode = po.BasisCode,
                BasisValue = po.BasisValue,
                BuyerCode = po.BuyerCode,               
                CountryCode = po.CountryCode,
                CurrencyCode = po.CurrencyCode,
                GarmentType = po.GarmentType,
                GarmentTypeName = gt.TypeName,
                Order = po.Order,
                OrderDate = po.OrderDate,
                Season = po.Season,
                TotalQuantity = po.TotalQuantity,
                UnitCode = po.UnitCode
            });

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
            //var PODbModel = await _apparelProDbContext.PurchaseOrders
            //    .Where(po => po.BuyerCode == buyer && po.Order == order)
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync();
            //var POServiceModel = _mapper.Map<PurchaseOrderServiceModel>(PODbModel);
            //return POServiceModel;
            throw new NotImplementedException();
        }

        public Task UpdatePurchaseOrderAsync(UpdatePurchaseOrderServiceModel updateCurrencyServiceModel)
        {
            throw new NotImplementedException();
        }
    }
}
