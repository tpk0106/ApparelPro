using apparelPro.BusinessLogic.Services.Implementation.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeDetailsService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorQuantityRatioService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderDetailReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderListReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOutstandingPurchaseOrderListReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.ITrimSheetReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IGarmentAdditionalCostService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.ISubContractService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IScheduledShipmentsReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IShipmentStatusReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IYearSeasonOrdersReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPendingEventsReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStockArrivalStatusReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.ICostOfProductionReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderQuotaDetailReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPostOrderCostSheetReportService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMonthlyActualShipmentsReportService;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.Reference.IAdditionalCostService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISubContractorService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBasisService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISeasonService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyConversionService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyExchangeService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService;
using apparelPro.BusinessLogic.Services.Models.Reference.IDepartmentService;
using apparelPro.BusinessLogic.Services.Models.Reference.IFeatureService;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeService;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeItemsService;
using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemFeatureService;
using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemCatalogService;
using apparelPro.BusinessLogic.Services.Models.Reference.IPortDestinationService;
using apparelPro.BusinessLogic.Services.Models.Reference.IStockService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitConversionService;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using apparelPro.BusinessLogic.Services.Models.Registration.IGroupService;
using apparelPro.BusinessLogic.Services.Models.Registration.IPermissionService;
using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using apparelPro.BusinessLogic.Services.Models.SystemConfiguration.ISystemParameterService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineService;
using apparelPro.BusinessLogic.Services.Models.Production.IOperationService;
using apparelPro.BusinessLogic.Services.Models.Production.INonProductiveHourCodeService;
using apparelPro.BusinessLogic.Services.Models.Production.IMachineTypeService;
using apparelPro.BusinessLogic.Services.Models.Production.IGarmentComponentService;
using apparelPro.BusinessLogic.Services.Models.Production.IEmployeeService;
using apparelPro.BusinessLogic.Services.Models.Production.IStyleComponentBreakdownService;
using apparelPro.BusinessLogic.Services.Models.Production.IStyleOperationBreakdownService;
using apparelPro.BusinessLogic.Services.Models.Production.IComponentOperationTemplateService;
using apparelPro.BusinessLogic.Services.Models.Production.IHolidayService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineAllocationService;
using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionLineAllocationService;
using apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionTimeTicketService;
using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionEntryService;
using apparelPro.BusinessLogic.Services.Models.Production.ISectionService;
using apparelPro.BusinessLogic.Services.Models.Production.IDailyProductionEntryService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryDailyReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionScheduleReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyOverviewReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseDetailedReportService;
using apparelPro.BusinessLogic.Services.Models.Production.ILineProductionSummaryReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IOperationBreakdownReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IManpowerRequirementReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IDailyEmployeeEfficiencyReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IMonthlyEmployeeEfficiencyReportService;
using apparelPro.BusinessLogic.Services.Models.Production.ILineEfficiencyReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionScheduleReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionAnalysisSummaryReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionProgressGraphService;
using apparelPro.BusinessLogic.Services.Models.Production.IEndOfProductionConfirmationService;
using apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService;
using ApparelPro.WebApi.APIModels.Dashboard;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using ApparelPro.Shared.Extensions;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels.Registration;
using ApparelPro.WebApi.APIModels.SystemConfiguration;
using ApparelPro.WebApi.Reports.Models;
using AutoMapper;

namespace ApparelPro.WebApi.Mappings
{
    public class ServicetoAPIModelMappings : Profile
    {
        public ServicetoAPIModelMappings()
        {
            // currency
            CreateMap<CurrencyServiceModel, CurrencyAPIModel>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForMember(src => src.CurrencyDetails, opt => opt.MapFrom(src => src.CurrencyDetails)).ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CurrencyAPIModel, UpdateCurrencyServiceModel>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<UpdateCurrencyAPIModel, UpdateCurrencyServiceModel>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor));

            CreateMap<CreateCurrencyAPIModel, CreateCurrencyServiceModel>().MaxDepth(2)
            .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
            .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode));

            // Bank
            CreateMap<CreateCountryAPIModel, CreateCountryServiceModel>().MaxDepth(2)
           .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
           .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
           .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag));

            CreateMap<UpdateCountryAPIModel, UpdateCountryServiceModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag));

            CreateMap<CountryServiceModel, CountryAPIModel>()
                .ReverseMap();

            // garment type
            CreateMap<CreateGarmentTypeAPIModel, CreateGarmentTypeServiceModel>()
                .ReverseMap();

            CreateMap<GarmentTypeServiceModel, GarmentTypeAPIModel>()
                .ReverseMap();

            CreateMap<UpdateGarmentTypeAPIModel, UpdateGarmentTypeServiceModel>().MaxDepth(2)
             .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName));

            // unit
            CreateMap<UnitServiceModel, UnitAPIModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<UpdateUnitAPIModel, UpdateUnitServiceModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<CreateUnitAPIModel, CreateUnitServiceModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            // unit conversion
            CreateMap<CreateUnitConversionAPIModel, UnitConversionServiceModel>().MaxDepth(2)
                .ForMember(src => src.FromUnit, opt => opt.MapFrom(src => src.FromUnit))
                .ForMember(src => src.ToUnit, opt => opt.MapFrom(src => src.ToUnit))
                .ForMember(src => src.Measure, opt => opt.MapFrom(src => src.Measure))
                .MaxDepth(2);

            CreateMap<UnitConversionServiceModel, UnitConversionAPIModel>().MaxDepth(2)
                .ForMember(src => src.FromUnit, opt => opt.MapFrom(src => src.FromUnit))
                .ForMember(src => src.ToUnit, opt => opt.MapFrom(src => src.ToUnit))
                .ForMember(src => src.Measure, opt => opt.MapFrom(src => src.Measure))
                .MaxDepth(2);

            // bank

            // 1. Map individual address entry structures
            CreateMap<CreateAddressAPIModel, CreateAddressServiceModel>();

            CreateMap<BankAPIModel, BankServiceModel>().MaxDepth(2)
                .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))
                .ForMember(src => src.SwiftCode, opt => opt.MapFrom(src => src.SwiftCode))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
                .ReverseMap();

            CreateMap<CreateBankAPIModel, CreateBankServiceModel>().MaxDepth(2)
                .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
                .ForMember(src => src.SwiftCode, opt => opt.MapFrom(src => src.SwiftCode))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses));

            CreateMap<CreateBankServiceModel, Bank>();

            CreateMap<UpdateBankAPIModel, UpdateBankServiceModel>().MaxDepth(2)
              .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))
              .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
              .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
              .ForMember(src => src.SwiftCode, opt => opt.MapFrom(src => src.SwiftCode))
              .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
              .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses));

            // Department
            CreateMap<DepartmentServiceModel, DepartmentAPIModel>().MaxDepth(2);

            // basis
            CreateMap<UpdateBasisServiceModel, BasisAPIModel>().MaxDepth(2);
            CreateMap<SeasonServiceModel, SeasonAPIModel>().MaxDepth(2);
            CreateMap<CreateSeasonServiceModel, CreateSeasonAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<SeasonServiceModel, CreateSeasonAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<UpdateSeasonAPIModel, UpdateSeasonServiceModel>().MaxDepth(2);

            CreateMap<BasisServiceModel, BasisAPIModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<CreateBasisServiceModel, CreateBasisAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<BasisServiceModel, CreateBasisAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<UpdateBasisAPIModel, UpdateBasisServiceModel>().MaxDepth(2);

            // additional cost
            CreateMap<AdditionalCostServiceModel, AdditionalCostAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<CreateAdditionalCostAPIModel, CreateAdditionalCostServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<UpdateAdditionalCostAPIModel, UpdateAdditionalCostServiceModel>().MaxDepth(2);

            // sub contractor (od_scref) - built 2026-08-09 as a prerequisite for AIN.
            CreateMap<SubContractorServiceModel, SubContractorAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<CreateSubContractorAPIModel, CreateSubContractorServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<UpdateSubContractorAPIModel, UpdateSubContractorServiceModel>().MaxDepth(2);

            // stock reference (RF_MENU.PRG > C. Inventory Control > A. Stock Reference)
            CreateMap<StockServiceModel, StockAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<CreateStockAPIModel, CreateStockServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<UpdateStockAPIModel, UpdateStockServiceModel>().MaxDepth(2);

            // order item catalog (od_itm master list)
            CreateMap<OrderItemCatalogServiceModel, OrderItemCatalogAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<CreateOrderItemCatalogAPIModel, CreateOrderItemCatalogServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<UpdateOrderItemCatalogAPIModel, UpdateOrderItemCatalogServiceModel>().MaxDepth(2);

            // garment additional cost
            CreateMap<GarmentAdditionalCostServiceModel, GarmentAdditionalCostAPIModel>().MaxDepth(2);
            CreateMap<SaveGarmentAdditionalCostAPIModel, SaveGarmentAdditionalCostServiceModel>().MaxDepth(2);
            CreateMap<GarmentAdditionalCostReportServiceModel, GarmentAdditionalCostReportAPIModel>().MaxDepth(2);
            CreateMap<GarmentAdditionalCostCategoryServiceModel, GarmentAdditionalCostCategoryAPIModel>().MaxDepth(2);
            CreateMap<GarmentAdditionalCostLineServiceModel, GarmentAdditionalCostLineAPIModel>().MaxDepth(2);

            // sub contract (Order Management -> D. Sub Contracts, od_subc1.prg) - 2026-08-09
            CreateMap<SubContractServiceModel, SubContractAPIModel>().MaxDepth(2);
            CreateMap<SaveSubContractAPIModel, SaveSubContractServiceModel>().MaxDepth(2);
            CreateMap<SaveSubContractResultServiceModel, SaveSubContractResultAPIModel>().MaxDepth(2);

            // garment type wise item requirements (RF_MENU.PRG > B. Order Management > G. Type / Item)
            CreateMap<GarmentTypeItemServiceModel, GarmentTypeItemAPIModel>().MaxDepth(2);
            CreateMap<SaveGarmentTypeItemAPIModel, SaveGarmentTypeItemServiceModel>().MaxDepth(2);

            // destination
            CreateMap<PortDestinationAPIModel, PortDestinationServiceModel>().MaxDepth(2)
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ReverseMap();

            CreateMap<CreatePortDestinationAPIModel, CreatePortDestinationServiceModel>().MaxDepth(2)
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code));

            CreateMap<CreatePortDestinationServiceModel, Destination>().MaxDepth(2)
              .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
              .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
              .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
              .ReverseMap();

            CreateMap<UpdatePortDestinationAPIModel, UpdatePortDestinationServiceModel>().MaxDepth(2)
              .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
              .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
              .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code));

            // PO
            CreateMap<PurchaseOrderServiceModel, POAPIModel>().MaxDepth(2)
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.BasisValue, opt => opt.MapFrom(src => src.BasisValue))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.TotalQuantity, opt => opt.MapFrom(src => src.TotalQuantity))
                .ForMember(src => src.UnitCode, opt => opt.MapFrom(src => src.UnitCode))
                .ForMember(src => src.GarmentType, opt => opt.MapFrom(src => src.GarmentType))
                .ForMember(src => src.GarmentTypeName, opt => opt.MapFrom(src => src.GarmentTypeName))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
                .ForMember(src => src.Season, opt => opt.MapFrom(src => src.Season))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ReverseMap();

            CreateMap<CreatePOAPIModel, CreatePurchaseOrderServiceModel>().MaxDepth(2)
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.BasisValue, opt => opt.MapFrom(src => src.BasisValue))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.TotalQuantity, opt => opt.MapFrom(src => src.TotalQuantity))
                .ForMember(src => src.UnitCode, opt => opt.MapFrom(src => src.UnitCode))
                .ForMember(src => src.GarmentType, opt => opt.MapFrom(src => src.GarmentType))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
                .ForMember(src => src.Season, opt => opt.MapFrom(src => src.Season))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode));

            // buyer
            CreateMap<BuyerServiceModel, BuyerAPIModel>().MaxDepth(2)
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CUSDEC, opt => opt.MapFrom(src => src.CUSDEC))
                .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
                .ReverseMap();

            CreateMap<CreateBuyerAPIModel, CreateBuyerServiceModel>().MaxDepth(2)
             .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
             .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
             .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
             .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
             .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(src => src.CUSDEC, opt => opt.MapFrom(src => src.CUSDEC))
             .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses));

            CreateMap<CreateBuyerServiceModel, Buyer>().MaxDepth(2)
            .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
            .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
            .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
            .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
            .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(src => src.CUSDEC, opt => opt.MapFrom(src => src.CUSDEC));

            CreateMap<UpdateBuyerAPIModel, UpdateBuyerServiceModel>().MaxDepth(2);

            // address
            CreateMap<AddressServiceModel, AddressAPIModel>().MaxDepth(2)
                .ReverseMap();
            CreateMap<UpdateAddressAPIModel, UpdateAddressServiceModel>().MaxDepth(2);
            CreateMap<CreateAddressAPIModel, CreateAddressServiceModel>().MaxDepth(2);
            // Needed by UpdateAddressByBuyerCodeAsync / UpdateAddressByBankCodeAsync, which
            // build a merged AddressAPIModel (fetched row + incoming edits) and map it
            // straight to UpdateAddressServiceModel before saving.
            CreateMap<AddressAPIModel, UpdateAddressServiceModel>().MaxDepth(2);

            // Supplier

            CreateMap<SupplierServiceModel, SupplierAPIModel>().MaxDepth(2)
                .ForMember(src => src.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                //   .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateSupplierAPIModel, CreateSupplierServiceModel>().MaxDepth(2)
            .ForMember(src => src.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<UpdateSupplierAPIModel, UpdateSupplierServiceModel>().MaxDepth(2)
              .ForMember(src => src.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
              .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
              .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
              .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
              .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
              .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
              .ReverseMap()
              .ForAllMembers(opt => opt.Ignore());

            // Feature (renamed to ItemFeature — matches ItemFeatureService/ItemFeatureController)

            CreateMap<ItemFeatureServiceModel, ItemFeatureAPIModel>().MaxDepth(2)
                .ForMember(src => src.FeatureCode, opt => opt.MapFrom(src => src.FeatureCode))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                 .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            // FIXED (2026-08-07): this warning's premise is stale - checked both classes
            // directly (CreateItemFeatureAPIModel and CreateItemFeatureServiceModel, both under
            // .../OrderManagement and .../Reference/IFeatureService respectively) and both
            // already have identical FeatureCode(string)/Description(string?) shapes; whatever
            // Id-vs-FeatureCode mismatch this note originally warned about no longer exists.
            // Found unregistered (never wired up after the warning was left) via a full sweep
            // of every controller's _mapper.Map<> call site - ItemFeatureController.AddFeatureAsync
            // would throw AutoMapperMappingException on every POST /api/item-feature.
            CreateMap<CreateItemFeatureAPIModel, CreateItemFeatureServiceModel>().MaxDepth(2);

            CreateMap<UpdateItemFeatureAPIModel, UpdateItemFeatureServiceModel>().MaxDepth(2)
                      .ForMember(src => src.FeatureCode, opt => opt.MapFrom(src => src.FeatureCode))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ReverseMap()
              .ForAllMembers(opt => opt.Ignore());


            // User           

            CreateMap<UserServiceModel, UserAPIModel>()
                .MaxDepth(2)
               .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
               .ForMember(src => src.Address, opt => opt.MapFrom(src => src.Address))
               .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.Address!.AddressId))
               .ForMember(src => src.LastActive, opt => opt.Ignore())
               .ForMember(src => src.PasswordHash, opt => opt.Ignore())
               .ForMember(src => src.Id, opt => opt.Ignore())
               .ReverseMap();

            CreateMap<CreateUserAPIModel, UserServiceModel>()
                .MaxDepth(2)
               .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
               .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
               .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
               .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
               .ReverseMap();

            CreateMap<UpdateUserAPIModel, UserServiceModel>()
                .MaxDepth(2)
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
               .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.ProfilePhoto))
               .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
               .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
               .ForMember(src => src.Address, opt => opt.MapFrom(src => src.Address));

            CreateMap<UpdateUserAPIModel, UpdateUserServiceModel>()
               .MaxDepth(2)
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(dest => dest.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
               .ForMember(dest => dest.ProfilePhoto, opt => opt.MapFrom(src => src.ProfilePhoto))
               .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
               .ForMember(dest => dest.LastActive, opt => opt.MapFrom(src => src.LastActive))
               .ForPath(dest => dest.Address!.StreetAddress, opt => opt.MapFrom(src => src.Address!.StreetAddress))
               .ForPath(dest => dest.Address!.City, opt => opt.MapFrom(src => src.Address!.City))
               .ForPath(dest => dest.Address!.PostCode, opt => opt.MapFrom(src => src.Address!.PostCode))
               .ForPath(dest => dest.Address!.State, opt => opt.MapFrom(src => src.Address!.State))
               .ForPath(dest => dest.Address!.CountryCode, opt => opt.MapFrom(src => src.Address!.CountryCode))
               .ForPath(dest => dest.Address!.AddressId, opt => opt.MapFrom(src => src.Address!.AddressId))
               .ForPath(dest => dest.Address!.Default, opt => opt.MapFrom(src => src.Address!.Default))
               .ForPath(dest => dest.Address!.AddressType, opt => opt.MapFrom(src => src.Address!.AddressType));

            // From API DTO to Service DTO
            CreateMap<RegisterUserAPIModel, RegisterUserServiceModel>().MaxDepth(2); // same as below mappings         

            CreateMap<ApparelProUser, RegisteredUserAPIModel>().MaxDepth(2)
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.ProfilePhoto));
            //.ForMember(src => src.Success, opt => opt.MapFrom(src => (src.Token != null && src.Token.Trim().Length > 0)));

            CreateMap<RegisterUserServiceModel, RegisteredUserAPIModel>().MaxDepth(2)
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(src => src.Success, opt => opt.MapFrom(src => (src.Token != null && src.Token.Trim().Length > 0)));

            CreateMap<RegisteredUserServiceModel, RegisteredUserAPIModel>().MaxDepth(2)
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.Token, opt => opt.MapFrom(src => src.Token))
                .ForMember(src => src.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(src => src.RefreshTokenExpiry, opt => opt.MapFrom(src => src.RefreshTokenExpiry))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(src => src.Success, opt => opt.MapFrom(src => (src.Token != null && src.Token.Trim().Length > 0)));

            CreateMap<LoginUserAPIModel, LoginUserServiceModel>().MaxDepth(2)
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.Password, opt => opt.MapFrom(src => src.Password));

            // currency exchange
            CreateMap<CreateCurrencyExchangeAPIModel, CreateCurrencyExchangeServiceModel>().MaxDepth(2)
                .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
                .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
                .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
                .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
                .ReverseMap();

            CreateMap<CurrencyExchangeAPIModel, CurrencyExchangeServiceModel>().MaxDepth(2)
                .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
                .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
                .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
                .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
                .ReverseMap();

            CreateMap<CurrencyExchange, CurrencyExchangeServiceModel>().MaxDepth(2)
              .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
              .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
              .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
              .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate));

            CreateMap<UpdateCurrencyExchangeAPIModel, CurrencyExchangeServiceModel>().MaxDepth(2)
              .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
              .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
              .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
              .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
              .ReverseMap();

            CreateMap<UpdateCurrencyExchangeAPIModel, UpdateCurrencyExchangeServiceModel>().MaxDepth(2)
             .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
             .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
             .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
             .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
             .ReverseMap();

            // currency conversion
            CreateMap<CurrencyConversionServiceModel, CurrencyConversionAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<CreateCurrencyConversionAPIModel, CreateCurrencyConversionServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<UpdateCurrencyConversionAPIModel, UpdateCurrencyConversionServiceModel>().MaxDepth(2).ReverseMap();

            //style
            CreateMap<StyleDetailsServiceModel, StyleAPIModel>().MaxDepth(2)
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
                .ForMember(src => src.StyleCode, opt => opt.MapFrom(src => src.StyleCode))
                .ForMember(src => src.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(src => src.Unit, opt => opt.MapFrom(src => src.Unit))
                .ForMember(src => src.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(src => src.ColorRatio, opt => opt.MapFrom(src => src.ColorRatio))
                .ForMember(src => src.SizeRatio, opt => opt.MapFrom(src => src.SizeRatio))
                .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<StyleDetailsServiceModel, StyleDetailsAPIModel>().MaxDepth(2)
               .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
               .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
               .ForMember(src => src.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
               .ForMember(src => src.StyleCode, opt => opt.MapFrom(src => src.StyleCode))
               .ForMember(src => src.Quantity, opt => opt.MapFrom(src => src.Quantity))
               .ForMember(src => src.Unit, opt => opt.MapFrom(src => src.Unit))
               .ForMember(src => src.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
               .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => src.OrderDate));

            CreateMap<CreateStyleDetailsAPIModel, CreateStyleDetailsServiceModel>().MaxDepth(2)
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
                .ForMember(src => src.StyleCode, opt => opt.MapFrom(src => src.StyleCode))
                .ForMember(src => src.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(src => src.Unit, opt => opt.MapFrom(src => src.Unit))
                .ForMember(src => src.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<StyleAPIModel, UpdateStyleDetailsServiceModel>().MaxDepth(2);

            CreateMap<UpdateStyleAPIModel, UpdateStyleDetailsServiceModel>().MaxDepth(2);

            CreateMap(typeof(PaginationResult<>), typeof(PaginationAPIModel<>)).MaxDepth(2)
                 .ConvertUsing(typeof(PaginationResultToPaginationAPITypeConverter<,>));

            // color/size details
            CreateMap<CreateColorSizeBreakdownDetailsAPIModel, CreateColorSizeBreakdownDetailsServiceModel>()
                .MaxDepth(2);
            CreateMap<ColorSizeBreakdownDetailsServiceModel, ColorSizeBreakdownDetailsAPIModel>()
                .MaxDepth(2);

            CreateMap<CreateColorQuantityRatioAPIModel, CreateColorQuantityRatioServiceModel>()
                .MaxDepth(2);
            CreateMap<ColorQuantityRatioServiceModel, ColorQuantityRatioAPIModel>()
                .MaxDepth(2);

            // material consumption

            CreateMap<CreateMaterialConsumptionEntryRequestAPIModel, CreateMaterialConsumptionEntryRequestServiceModel>().MaxDepth(2);
            CreateMap<CopyMaterialsFromStyleRequestAPIModel, CopyMaterialsFromStyleRequestServiceModel>().MaxDepth(2);
            CreateMap<CopyMaterialsFromStyleResultServiceModel, CopyMaterialsFromStyleResultAPIModel>().MaxDepth(2);
            CreateMap<MaterialCatalogItemServiceModel, MaterialCatalogItemAPIModel>().MaxDepth(2);
            CreateMap<MaterialCatalogGroupServiceModel, MaterialCatalogGroupAPIModel>().MaxDepth(2);
            CreateMap<StyleMaterialConsumptionLedgerRowServiceModel, StyleMaterialConsumptionLedgerRowAPIModel>().MaxDepth(2);
            CreateMap<OrderItemFeature, OrderItemFeatureServiceModel>().MaxDepth(2);
            CreateMap<OrderItemFeatureServiceModel, OrderItemFeatureAPIModel>()
                .ForMember(src => src.ItemCode, opt => opt.MapFrom(src => src.ItemCode))
                .ForMember(src => src.StockCode, opt => opt.MapFrom(src => src.StockCode))
                .ForMember(src => src.Feature1, opt => opt.MapFrom(src => src.Feature1Label))
                .ForMember(src => src.Feature2, opt => opt.MapFrom(src => src.Feature2Label))
                .ForMember(src => src.Feature3, opt => opt.MapFrom(src => src.Feature3Label))
                .ForMember(src => src.Feature4, opt => opt.MapFrom(src => src.Feature4Label))
                .ForMember(src => src.CostPerUnit, opt => opt.MapFrom(src => src.CostPerUnit))
                .MaxDepth(2);

            CreateMap<OrderItemFeatureMappingServiceModel, OrderItemFeatureMappingAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<CreateOrderItemFeatureMappingAPIModel, CreateOrderItemFeatureMappingServiceModel>().MaxDepth(2);
            CreateMap<UpdateOrderItemFeatureMappingAPIModel, UpdateOrderItemFeatureMappingServiceModel>().MaxDepth(2);

            CreateMap<StyleDimensionsLookupServiceModel, StyleDimensionsLookupAPIModel>().MaxDepth(2);
            CreateMap<SupplierLookupServiceModel, SupplierLookupAPIModel>().MaxDepth(2);

            CreateMap<StyleApprovalDetailsServiceModel, StyleApprovalDetailsAPIModel>().MaxDepth(2);

            // Trim Sheet Report (2026-08-07)
            CreateMap<TrimSheetReportServiceModel, TrimSheetReportAPIModel>().MaxDepth(2);
            CreateMap<TrimSheetLineServiceModel, TrimSheetLineAPIModel>().MaxDepth(2);
            CreateMap<TrimSheetStockGroupServiceModel, TrimSheetStockGroupAPIModel>().MaxDepth(2);
            CreateMap<TrimSheetSupplierTotalServiceModel, TrimSheetSupplierTotalAPIModel>().MaxDepth(2);
            CreateMap<TrimSheetProfitServiceModel, TrimSheetProfitAPIModel>().MaxDepth(2);
            CreateMap<TrimSheetApprovalStampServiceModel, TrimSheetApprovalStampAPIModel>().MaxDepth(2);

            // Order Detail Report (2026-08-07)
            CreateMap<OrderDetailReportServiceModel, OrderDetailReportAPIModel>().MaxDepth(2);
            CreateMap<OrderDetailStyleServiceModel, OrderDetailStyleAPIModel>().MaxDepth(2);
            CreateMap<OrderDetailPartShipmentServiceModel, OrderDetailPartShipmentAPIModel>().MaxDepth(2);

            // Colour/Size Report (2026-08-08)
            CreateMap<ColorSizeReportServiceModel, ColorSizeReportAPIModel>().MaxDepth(2);
            CreateMap<ColorSizeReportStyleServiceModel, ColorSizeReportStyleAPIModel>().MaxDepth(2);
            CreateMap<ColorSizeReportColourServiceModel, ColorSizeReportColourAPIModel>().MaxDepth(2);

            // Scheduled Shipments Report
            CreateMap<ScheduledShipmentRowServiceModel, ScheduledShipmentRowAPIModel>().MaxDepth(2);
            CreateMap<ScheduledShipmentsReportServiceModel, ScheduledShipmentsReportAPIModel>().MaxDepth(2);

            // Order/Quota Detail Report
            CreateMap<OrderQuotaDetailRowServiceModel, OrderQuotaDetailRowAPIModel>().MaxDepth(2);
            CreateMap<OrderQuotaDetailReportServiceModel, OrderQuotaDetailReportAPIModel>().MaxDepth(2);

            // Post Order Cost Sheet Report
            CreateMap<PostOrderCostSheetStyleServiceModel, PostOrderCostSheetStyleAPIModel>().MaxDepth(2);
            CreateMap<PostOrderCostSheetSectionQuantityServiceModel, PostOrderCostSheetSectionQuantityAPIModel>().MaxDepth(2);
            CreateMap<PostOrderCostSheetMaterialGroupServiceModel, PostOrderCostSheetMaterialGroupAPIModel>().MaxDepth(2);
            CreateMap<PostOrderCostSheetAdditionalCostGroupServiceModel, PostOrderCostSheetAdditionalCostGroupAPIModel>().MaxDepth(2);
            CreateMap<PostOrderCostSheetReportServiceModel, PostOrderCostSheetReportAPIModel>().MaxDepth(2);

            // Monthly Actual Shipments Report
            CreateMap<MonthlyActualShipmentRowServiceModel, MonthlyActualShipmentRowAPIModel>().MaxDepth(2);
            CreateMap<MonthlyActualShipmentsReportServiceModel, MonthlyActualShipmentsReportAPIModel>().MaxDepth(2);

            // Shipment Status Report
            CreateMap<ShipmentStatusInvoiceLineServiceModel, ShipmentStatusInvoiceLineAPIModel>().MaxDepth(2);
            CreateMap<ShipmentStatusRowServiceModel, ShipmentStatusRowAPIModel>().MaxDepth(2);
            CreateMap<ShipmentStatusReportServiceModel, ShipmentStatusReportAPIModel>().MaxDepth(2);

            // Year/Season Wise Orders Report
            CreateMap<YearSeasonOrderStyleServiceModel, YearSeasonOrderStyleAPIModel>().MaxDepth(2);
            CreateMap<YearSeasonOrderRowServiceModel, YearSeasonOrderRowAPIModel>().MaxDepth(2);
            CreateMap<YearSeasonOrdersReportServiceModel, YearSeasonOrdersReportAPIModel>().MaxDepth(2);

            // Pending Events Report
            CreateMap<PendingEventRowServiceModel, PendingEventRowAPIModel>().MaxDepth(2);
            CreateMap<PendingEventStyleGroupServiceModel, PendingEventStyleGroupAPIModel>().MaxDepth(2);
            CreateMap<PendingEventsReportServiceModel, PendingEventsReportAPIModel>().MaxDepth(2);

            // Stock Arrival Status Report
            CreateMap<StockArrivalPoLineServiceModel, StockArrivalPoLineAPIModel>().MaxDepth(2);
            CreateMap<StockArrivalItemServiceModel, StockArrivalItemAPIModel>().MaxDepth(2);
            CreateMap<StockArrivalStatusReportServiceModel, StockArrivalStatusReportAPIModel>().MaxDepth(2);

            // Cost of Production Report
            CreateMap<CostOfProductionMaterialLineServiceModel, CostOfProductionMaterialLineAPIModel>().MaxDepth(2);
            CreateMap<CostOfProductionAdditionalCostLineServiceModel, CostOfProductionAdditionalCostLineAPIModel>().MaxDepth(2);
            CreateMap<CostOfProductionAdditionalCostGroupServiceModel, CostOfProductionAdditionalCostGroupAPIModel>().MaxDepth(2);
            CreateMap<CostOfProductionSubContractLineServiceModel, CostOfProductionSubContractLineAPIModel>().MaxDepth(2);
            CreateMap<CostOfProductionStyleRevenueServiceModel, CostOfProductionStyleRevenueAPIModel>().MaxDepth(2);
            CreateMap<CostOfProductionLineCostServiceModel, CostOfProductionLineCostAPIModel>().MaxDepth(2);
            CreateMap<CostOfProductionReportServiceModel, CostOfProductionReportAPIModel>().MaxDepth(2);

            // Purchase Order List Report (2026-08-08)
            CreateMap<PurchaseOrderListReportServiceModel, PurchaseOrderListReportAPIModel>().MaxDepth(2);
            CreateMap<PurchaseOrderListLineServiceModel, PurchaseOrderListLineReportAPIModel>().MaxDepth(2);

            // Outstanding Purchase Order List Report (2026-08-08)
            CreateMap<OutstandingPurchaseOrderListReportServiceModel, OutstandingPurchaseOrderListReportAPIModel>().MaxDepth(4);
            CreateMap<OutstandingPurchaseOrderBasisGroupServiceModel, OutstandingPurchaseOrderBasisGroupReportAPIModel>().MaxDepth(4);
            CreateMap<OutstandingPurchaseOrderServiceModel, OutstandingPurchaseOrderReportAPIModel>().MaxDepth(4);
            CreateMap<OutstandingPurchaseOrderGroupServiceModel, OutstandingPurchaseOrderGroupReportAPIModel>().MaxDepth(4);

            // orderwise inventory
            CreateMap<RequisitionHeaderAPIModel, RequisitionHeaderServiceModel>().MaxDepth(2);
            CreateMap<RequisitionLineItemAPIModel, RequisitionLineItemServiceModel>().MaxDepth(2);
            CreateMap<STRNAPIModel, STRNServiceModel>().MaxDepth(2);
            CreateMap<StockItemAvailabilityDetails, StockItemAvailabilityAPIModel>().MaxDepth(2);
            CreateMap<OrderwiseStockLookupRowServiceModel, StockLookupRowAPIModel>().MaxDepth(2);

            // orderwise inventory - STRN print
            CreateMap<StrnPrintHeaderServiceModel, StrnPrintHeaderAPIModel>().MaxDepth(2);
            CreateMap<StrnPrintLineServiceModel, StrnPrintLineAPIModel>().MaxDepth(2);
            CreateMap<StrnPrintDetailsServiceModel, StrnPrintDetailsAPIModel>().MaxDepth(2);

            // orderwise inventory - GIN
            CreateMap<GinHeaderAPIModel, GinHeaderServiceModel>().MaxDepth(2);
            CreateMap<GinLineItemAPIModel, GinLineItemServiceModel>().MaxDepth(2);
            CreateMap<GinIssuableStrnLineServiceModel, GinIssuableStrnLineAPIModel>().MaxDepth(2);
            CreateMap<GinStrnLookupResultServiceModel, GinStrnLookupResultAPIModel>().MaxDepth(2);
            CreateMap<GinPendingStrnServiceModel, GinPendingStrnAPIModel>().MaxDepth(2);

            // orderwise inventory - GRN
            CreateMap<GrnHeaderAPIModel, GrnHeaderServiceModel>().MaxDepth(2);
            CreateMap<GrnLineItemAPIModel, GrnLineItemServiceModel>().MaxDepth(2);
            CreateMap<GrnReceivableLineServiceModel, GrnReceivableLineAPIModel>().MaxDepth(2);
            CreateMap<GrnPoLookupResultServiceModel, GrnPoLookupResultAPIModel>().MaxDepth(2);
            CreateMap<GrnPendingPoServiceModel, GrnPendingPoAPIModel>().MaxDepth(2);

            // orderwise inventory - RTN (Goods Return Note)
            CreateMap<RtnHeaderAPIModel, RtnHeaderServiceModel>().MaxDepth(2);
            CreateMap<RtnLineItemAPIModel, RtnLineItemServiceModel>().MaxDepth(2);
            CreateMap<RtnReturnableStockRowServiceModel, RtnReturnableStockRowAPIModel>().MaxDepth(2);

            // orderwise inventory - SAN (Stock Adjustment Note)
            // FIXED (2026-08-07): all three were missing entirely - same class of bug as the
            // Permissions maps noted below (confirmed by grepping this file and
            // DatabaseToServiceMappings.cs for "San"/"StockAdjustment" and finding zero hits).
            // SANController.GetAdjustableStock's Map<List<SanAdjustableStockRowAPIModel>>(...)
            // was throwing AutoMapperMappingException on every call - reported by the user as
            // "Failed to load adjustable stock: Error mapping types...". CommitStockAdjustment's
            // two Header/Lines maps were equally unregistered but hadn't been hit yet.
            CreateMap<SanAdjustableStockRowServiceModel, SanAdjustableStockRowAPIModel>().MaxDepth(2);
            CreateMap<SanHeaderAPIModel, SanHeaderServiceModel>().MaxDepth(2);
            CreateMap<SanLineItemAPIModel, SanLineItemServiceModel>().MaxDepth(2);

            // orderwise inventory - AIN (Additional Issue Note)
            CreateMap<AinIssuableStockRowServiceModel, AinIssuableStockRowAPIModel>().MaxDepth(2);
            CreateMap<AinHeaderAPIModel, AinHeaderServiceModel>().MaxDepth(2);
            CreateMap<AinLineItemAPIModel, AinLineItemServiceModel>().MaxDepth(2);

            // orderwise inventory - DGN (Damaged Goods Note)
            // FIXED (2026-08-07): found via a full sweep of every controller's _mapper.Map<>
            // call site after the SAN bug above - same class of gap, never registered at all.
            CreateMap<DgnDamageableStockRowServiceModel, DgnDamageableStockRowAPIModel>().MaxDepth(2);
            CreateMap<DgnHeaderAPIModel, DgnHeaderServiceModel>().MaxDepth(2);
            CreateMap<DgnLineItemAPIModel, DgnLineItemServiceModel>().MaxDepth(2);

            // orderwise inventory - GTN (Goods Transfer Note)
            // FIXED (2026-08-07): same sweep, same gap.
            CreateMap<GtnTransferableStockRowServiceModel, GtnTransferableStockRowAPIModel>().MaxDepth(2);
            CreateMap<GtnHeaderAPIModel, GtnHeaderServiceModel>().MaxDepth(2);
            CreateMap<GtnLineItemAPIModel, GtnLineItemServiceModel>().MaxDepth(2);

            // orderwise inventory - SRN (Supplier Return Note)
            // FIXED (2026-08-07): same sweep, same gap.
            CreateMap<SrnReturnableStockRowServiceModel, SrnReturnableStockRowAPIModel>().MaxDepth(2);
            CreateMap<SrnHeaderAPIModel, SrnHeaderServiceModel>().MaxDepth(2);
            CreateMap<SrnLineItemAPIModel, SrnLineItemServiceModel>().MaxDepth(2);

            // orderwise inventory - stock movement report
            CreateMap<StockMovementReportHeaderServiceModel, StockMovementReportHeaderAPIModel>().MaxDepth(2);
            CreateMap<StockMovementReportLineServiceModel, StockMovementReportLineAPIModel>().MaxDepth(2);

            // orderwise inventory - stock movement report (for an item)
            CreateMap<StockMovementItemOptionServiceModel, StockMovementItemOptionAPIModel>().MaxDepth(2);
            CreateMap<StockMovementItemReportHeaderServiceModel, StockMovementItemReportHeaderAPIModel>().MaxDepth(2);
            CreateMap<StockMovementItemReportLineServiceModel, StockMovementItemReportLineAPIModel>().MaxDepth(2);

            // order confirmation - style totals / system configuration
            CreateMap<StyleTotalsServiceModel, StyleTotalsAPIModel>().MaxDepth(2);
            CreateMap<SystemParameterServiceModel, SystemParameterAPIModel>().MaxDepth(2);

            // groups & user-group management (new, 2026-08-03)
            CreateMap<GroupServiceModel, GroupAPIModel>().MaxDepth(2);
            CreateMap<UserWithGroupsServiceModel, UserWithGroupsAPIModel>().MaxDepth(2);

            // permissions - these three were missing entirely; PermissionsController would have
            // thrown on first real use (AutoMapperMappingException - confirmed by grepping this
            // file for any existing Permission-related CreateMap and finding none). Fixed
            // alongside the Groups work since the Users & Groups screen's Permission Matrix
            // panel depends on this controller actually working.
            CreateMap<PermissionServiceModel, PermissionAPIModel>().MaxDepth(2);
            CreateMap<RolePermissionMatrixRoleServiceModel, RolePermissionMatrixRoleAPIModel>().MaxDepth(2);
            CreateMap<UpdateRolePermissionsAPIModel, UpdateRolePermissionsServiceModel>().MaxDepth(2);

            // production control
            CreateMap<CreateProductionLineAPIModel, CreateProductionLineServiceModel>().MaxDepth(2);
            CreateMap<UpdateProductionLineAPIModel, UpdateProductionLineServiceModel>().MaxDepth(2);
            CreateMap<ProductionLineServiceModel, ProductionLineAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateOperationAPIModel, CreateOperationServiceModel>().MaxDepth(2);
            CreateMap<UpdateOperationAPIModel, UpdateOperationServiceModel>().MaxDepth(2);
            CreateMap<OperationServiceModel, OperationAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateNonProductiveHourCodeAPIModel, CreateNonProductiveHourCodeServiceModel>().MaxDepth(2);
            CreateMap<UpdateNonProductiveHourCodeAPIModel, UpdateNonProductiveHourCodeServiceModel>().MaxDepth(2);
            CreateMap<NonProductiveHourCodeServiceModel, NonProductiveHourCodeAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateMachineTypeAPIModel, CreateMachineTypeServiceModel>().MaxDepth(2);
            CreateMap<UpdateMachineTypeAPIModel, UpdateMachineTypeServiceModel>().MaxDepth(2);
            CreateMap<MachineTypeServiceModel, MachineTypeAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateGarmentComponentAPIModel, CreateGarmentComponentServiceModel>().MaxDepth(2);
            CreateMap<UpdateGarmentComponentAPIModel, UpdateGarmentComponentServiceModel>().MaxDepth(2);
            CreateMap<GarmentComponentServiceModel, GarmentComponentAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateEmployeeAPIModel, CreateEmployeeServiceModel>().MaxDepth(2);
            CreateMap<UpdateEmployeeAPIModel, UpdateEmployeeServiceModel>().MaxDepth(2);
            CreateMap<EmployeeServiceModel, EmployeeAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateStyleComponentBreakdownAPIModel, CreateStyleComponentBreakdownServiceModel>().MaxDepth(2);
            CreateMap<StyleComponentBreakdownServiceModel, StyleComponentBreakdownAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateStyleOperationBreakdownAPIModel, CreateStyleOperationBreakdownServiceModel>().MaxDepth(2);
            CreateMap<StyleOperationBreakdownServiceModel, StyleOperationBreakdownAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<StyleOperationBreakdownSaveResultServiceModel, StyleOperationBreakdownSaveResultAPIModel>().MaxDepth(2);

            CreateMap<CreateComponentOperationTemplateAPIModel, CreateComponentOperationTemplateServiceModel>().MaxDepth(2);
            CreateMap<UpdateComponentOperationTemplateAPIModel, UpdateComponentOperationTemplateServiceModel>().MaxDepth(2);
            CreateMap<ComponentOperationTemplateServiceModel, ComponentOperationTemplateAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateHolidayAPIModel, CreateHolidayServiceModel>().MaxDepth(2);
            CreateMap<HolidayServiceModel, HolidayAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<ManualAllocateProductionLineAPIModel, ManualAllocateProductionLineServiceModel>().MaxDepth(2);
            CreateMap<AutomaticAllocateProductionLineAPIModel, AutomaticAllocateProductionLineServiceModel>().MaxDepth(2);
            CreateMap<ProductionLineAllocationServiceModel, ProductionLineAllocationAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<ProductionLineAllocationResultServiceModel, ProductionLineAllocationResultAPIModel>().MaxDepth(2);

            CreateMap<ManualAllocateEstimatedProductionLineAPIModel, ManualAllocateEstimatedProductionLineServiceModel>().MaxDepth(2);
            CreateMap<AutomaticAllocateEstimatedProductionLineAPIModel, AutomaticAllocateEstimatedProductionLineServiceModel>().MaxDepth(2);
            CreateMap<EstimatedProductionLineAllocationServiceModel, EstimatedProductionLineAllocationAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<EstimatedProductionLineAllocationResultServiceModel, EstimatedProductionLineAllocationResultAPIModel>().MaxDepth(2);

            CreateMap<CreateDailyProductionTimeTicketEntryAPIModel, CreateDailyProductionTimeTicketEntryServiceModel>().MaxDepth(2);
            CreateMap<DailyProductionTimeTicketEntryServiceModel, DailyProductionTimeTicketEntryAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<EmployeeEfficiencySummaryServiceModel, EmployeeEfficiencySummaryAPIModel>().MaxDepth(2);
            CreateMap<DailyProductionTimeTicketServiceModel, DailyProductionTimeTicketAPIModel>().MaxDepth(2);

            CreateMap<CreateEstimatedProductionEntryAPIModel, CreateEstimatedProductionEntryServiceModel>().MaxDepth(2);
            CreateMap<EstimatedProductionEntryServiceModel, EstimatedProductionEntryAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateSectionAPIModel, CreateSectionServiceModel>().MaxDepth(2);
            CreateMap<UpdateSectionAPIModel, UpdateSectionServiceModel>().MaxDepth(2);
            CreateMap<SectionServiceModel, SectionAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CreateDailyProductionEntryAPIModel, CreateDailyProductionEntryServiceModel>().MaxDepth(2);
            CreateMap<DailyProductionEntryServiceModel, DailyProductionEntryAPIModel>().MaxDepth(2).ReverseMap();

            CreateMap<CurrentStyleServiceModel, CurrentStyleAPIModel>().MaxDepth(2);
            CreateMap<SectionProgressServiceModel, SectionProgressAPIModel>().MaxDepth(2);
            CreateMap<ProductionProgressServiceModel, ProductionProgressAPIModel>().MaxDepth(2);
            CreateMap<DailyTrendPointServiceModel, DailyTrendPointAPIModel>().MaxDepth(2);
            CreateMap<DailyTrendSeriesServiceModel, DailyTrendSeriesAPIModel>().MaxDepth(2);
            CreateMap<ColorSizeMixServiceModel, ColorSizeMixAPIModel>().MaxDepth(2);
            CreateMap<OrderManagementSummaryServiceModel, OrderManagementSummaryAPIModel>().MaxDepth(2);
            CreateMap<StockItemMovementServiceModel, StockItemMovementAPIModel>().MaxDepth(2);
            CreateMap<OrderwiseInventorySummaryServiceModel, OrderwiseInventorySummaryAPIModel>().MaxDepth(2);

            CreateMap<ProductionSummaryDailySectionTotalServiceModel, ProductionSummaryDailySectionTotalAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryDailyLineServiceModel, ProductionSummaryDailyLineAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryDailyReportServiceModel, ProductionSummaryDailyReportAPIModel>().MaxDepth(2);

            CreateMap<ProductionScheduleLineServiceModel, ProductionScheduleLineAPIModel>().MaxDepth(2);
            CreateMap<ProductionScheduleReportServiceModel, ProductionScheduleReportAPIModel>().MaxDepth(2);

            CreateMap<ProductionSummaryMonthlyCellServiceModel, ProductionSummaryMonthlyCellAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryMonthlySubRowServiceModel, ProductionSummaryMonthlySubRowAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryMonthlyDayRowServiceModel, ProductionSummaryMonthlyDayRowAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryMonthlyReportServiceModel, ProductionSummaryMonthlyReportAPIModel>().MaxDepth(2);

            CreateMap<ProductionSummaryMonthlyOverviewCellServiceModel, ProductionSummaryMonthlyOverviewCellAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryMonthlyOverviewDayRowServiceModel, ProductionSummaryMonthlyOverviewDayRowAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryMonthlyOverviewReportServiceModel, ProductionSummaryMonthlyOverviewReportAPIModel>().MaxDepth(2);

            CreateMap<ProductionSummaryStyleWiseSectionQtyServiceModel, ProductionSummaryStyleWiseSectionQtyAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryStyleWiseRowServiceModel, ProductionSummaryStyleWiseRowAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryStyleWiseReportServiceModel, ProductionSummaryStyleWiseReportAPIModel>().MaxDepth(2);

            CreateMap<ProductionSummaryStyleWiseDetailedLineServiceModel, ProductionSummaryStyleWiseDetailedLineAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryStyleWiseDetailedRowServiceModel, ProductionSummaryStyleWiseDetailedRowAPIModel>().MaxDepth(2);
            CreateMap<ProductionSummaryStyleWiseDetailedReportServiceModel, ProductionSummaryStyleWiseDetailedReportAPIModel>().MaxDepth(2);

            CreateMap<LineProductionSummaryRowServiceModel, LineProductionSummaryRowAPIModel>().MaxDepth(2);
            CreateMap<LineProductionSummaryReportServiceModel, LineProductionSummaryReportAPIModel>().MaxDepth(2);

            CreateMap<OperationBreakdownRowServiceModel, OperationBreakdownRowAPIModel>().MaxDepth(2);
            CreateMap<OperationBreakdownComponentGroupServiceModel, OperationBreakdownComponentGroupAPIModel>().MaxDepth(2);
            CreateMap<OperationBreakdownReportServiceModel, OperationBreakdownReportAPIModel>().MaxDepth(2);

            CreateMap<ManpowerMachineTimeRowServiceModel, ManpowerMachineTimeRowAPIModel>().MaxDepth(2);
            CreateMap<ManpowerRequirementReportServiceModel, ManpowerRequirementReportAPIModel>().MaxDepth(2);

            CreateMap<EmployeeEfficiencyRowServiceModel, EmployeeEfficiencyRowAPIModel>().MaxDepth(2);
            CreateMap<DailyEmployeeEfficiencyReportServiceModel, DailyEmployeeEfficiencyReportAPIModel>().MaxDepth(2);

            CreateMap<EmployeeMonthlyEfficiencyDayCellServiceModel, EmployeeMonthlyEfficiencyDayCellAPIModel>().MaxDepth(2);
            CreateMap<EmployeeMonthlyEfficiencyRowServiceModel, EmployeeMonthlyEfficiencyRowAPIModel>().MaxDepth(2);
            CreateMap<MonthlyEmployeeEfficiencyReportServiceModel, MonthlyEmployeeEfficiencyReportAPIModel>().MaxDepth(2);

            CreateMap<LineEfficiencyDayCellServiceModel, LineEfficiencyDayCellAPIModel>().MaxDepth(2);
            CreateMap<LineEfficiencyReportServiceModel, LineEfficiencyReportAPIModel>().MaxDepth(2);

            CreateMap<EstimatedProductionScheduleRowServiceModel, EstimatedProductionScheduleRowAPIModel>().MaxDepth(2);
            CreateMap<EstimatedProductionScheduleReportServiceModel, EstimatedProductionScheduleReportAPIModel>().MaxDepth(2);

            CreateMap<ProductionAnalysisSectionQtyServiceModel, ProductionAnalysisSectionQtyAPIModel>().MaxDepth(2);
            CreateMap<ProductionAnalysisRowServiceModel, ProductionAnalysisRowAPIModel>().MaxDepth(2);
            CreateMap<ProductionAnalysisSummaryReportServiceModel, ProductionAnalysisSummaryReportAPIModel>().MaxDepth(2);

            CreateMap<ProductionProgressPointServiceModel, ProductionProgressPointAPIModel>().MaxDepth(2);
            CreateMap<ProductionProgressGraphServiceModel, ProductionProgressGraphAPIModel>().MaxDepth(2);

            CreateMap<EndOfProductionStatusServiceModel, EndOfProductionStatusAPIModel>().MaxDepth(2);
        }

        public class PaginationResultToPaginationAPITypeConverter<sourceT, destT> : ITypeConverter<PaginationResult<sourceT>, PaginationAPIModel<destT>>
           where sourceT : class
           where destT : class
        {
            public PaginationAPIModel<destT> Convert(PaginationResult<sourceT> source, PaginationAPIModel<destT> destination, ResolutionContext context)
            {
                if (destination == null)
                {
                    destination = new PaginationAPIModel<destT>();
                }
                destination.CurrentPage = source.CurrentPage;
                destination.PageSize = source.PageSize;
                destination.TotalItems = source.TotalItems;
                destination.TotalPages = source.TotalPages;
                destination.SortColumn = source.SortColumn;
                destination.SortOrder = source.SortOrder;
                destination.FilterColumn = source.FilterColumn;
                context.Mapper.Map(source.Items, destination.Items);
                return destination;
            }
        }
    }
}
