using apparelPro.BusinessLogic.Services.Implementation.Shared;

using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeDetailsService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorQuantityRatioService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
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
using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemFeatureService;
using apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemCatalogService;
using apparelPro.BusinessLogic.Services.Models.Reference.IPortDestinationService;
using apparelPro.BusinessLogic.Services.Models.Reference.IStockService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitConversionService;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
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
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Models.Production;
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using ApparelPro.Data.Models.SystemConfiguration;
using ApparelPro.Data.Models.Toolbar;
using apparelPro.BusinessLogic.Services.Models.Toolbar.IToolbarService;
using ApparelPro.Data.Models.ImportExport;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICompanyAddressService;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICommercialInvoiceService;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICertificateOfOriginService;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditService;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ILetterOfCreditCoveringLetterService;
using ApparelPro.WebApi.APIModels.OrderManagement;
using AutoMapper;

namespace apparelPro.BusinessLogic.Services.Mappings
{
    public class DatabaseToServiceMappings : Profile
    {
        public DatabaseToServiceMappings()
        {
            // toolbar
            CreateMap<ToolbarPin, ToolbarPinServiceModel>().MaxDepth(2);

            // import/export documentation
            CreateMap<CompanyAddress, CompanyAddressServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<CommercialInvoiceHeader, CommercialInvoiceHeaderServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<CommercialInvoiceLine, CommercialInvoiceLineServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<CertificateOfOriginHeader, CertificateOfOriginHeaderServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<CertificateOfOriginLine, CertificateOfOriginLineServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<LetterOfCreditHeader, LetterOfCreditHeaderServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<LetterOfCreditLine, LetterOfCreditLineServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<LetterOfCreditCoveringLetter, LetterOfCreditCoveringLetterServiceModel>().MaxDepth(2).ReverseMap();

            // currency
            CreateMap<Currency, CurrencyServiceModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.CurrencyDetails, opt => opt.MapFrom(src => src.CurrencyDetails))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateCurrencyServiceModel, Currency>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<UpdateCurrencyServiceModel, Currency>().MaxDepth(2)
               .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
               .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
               .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
               .ForAllMembers(opt => opt.Ignore());

            // Country
            CreateMap<Country, CountryServiceModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateCountryServiceModel, Country>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<UpdateCountryServiceModel, Country>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag));

            // Bank
            CreateMap<BankServiceModel, Bank>().MaxDepth(2)
                .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))
                .ForMember(src => src.SwiftCode, opt => opt.MapFrom(src => src.SwiftCode))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
                .ReverseMap();

            CreateMap<CreateBankServiceModel, Bank>().MaxDepth(2)
               .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))
               .ForMember(src => src.SwiftCode, opt => opt.MapFrom(src => src.SwiftCode))
               .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
               .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
               .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
               .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
               .ReverseMap();

            CreateMap<UpdateBankServiceModel, Bank>().MaxDepth(2)
            .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))
            .ForMember(src => src.SwiftCode, opt => opt.MapFrom(src => src.SwiftCode))
            .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
            .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
            .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
            .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
            .ReverseMap();

            // destination
            CreateMap<PortDestinationServiceModel, Destination>().MaxDepth(2)
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ReverseMap();

            // buyer
            CreateMap<Buyer, BuyerServiceModel>().MaxDepth(2)
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))                
                .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CUSDEC, opt => opt.MapFrom(src => src.CUSDEC))
                .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateBuyerServiceModel, Buyer>().MaxDepth(2)
              .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
              .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
              .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
              .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
              .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(src => src.CUSDEC, opt => opt.MapFrom(src => src.CUSDEC))
              .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
              .ReverseMap();

            // Department
            CreateMap<DepartmentServiceModel, Department>().ReverseMap().MaxDepth(2);

            // Unit
            CreateMap<Unit, UnitServiceModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateUnitServiceModel, Unit>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            // unit conversion
            CreateMap<UnitConversion, UnitConversionServiceModel>().MaxDepth(2)
                .ForMember(src => src.FromUnit, opt => opt.MapFrom(src => src.FromUnit))
                .ForMember(src => src.ToUnit, opt => opt.MapFrom(src => src.ToUnit))
                .ForMember(src => src.Measure, opt => opt.MapFrom(src => src.Measure))
                .MaxDepth(2)
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());


            // Garment Type
            CreateMap<CreateGarmentTypeServiceModel, GarmentType>().MaxDepth(2)
            .ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName));

            //CreateMap<GarmentTypeServiceModel, GarmentType>().MaxDepth(2)                
            //.ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName));

            CreateMap<GarmentType, GarmentTypeServiceModel>().MaxDepth(2)
              .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
          .ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName));

            CreateMap<GarmentType, GarmentTypeServiceModel>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName));

            CreateMap<UpdateGarmentTypeServiceModel, GarmentType>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName))
                .ReverseMap()                
                .ForAllMembers(opt => opt.Ignore());

            // basis

            CreateMap<Basis, BasisServiceModel>().MaxDepth(2);
            CreateMap<CreateBasisServiceModel,Basis>().MaxDepth(2);
            CreateMap<UpdateBasisServiceModel,Basis>().MaxDepth(2);

            // season (od_sea) - reference data for the Season dropdown
            CreateMap<Season, SeasonServiceModel>().MaxDepth(2);
            CreateMap<CreateSeasonServiceModel, Season>().MaxDepth(2);
            CreateMap<UpdateSeasonServiceModel, Season>().MaxDepth(2);

            // additional cost
            CreateMap<AdditionalCost, AdditionalCostServiceModel>().MaxDepth(2);
            CreateMap<CreateAdditionalCostServiceModel, AdditionalCost>().MaxDepth(2);
            CreateMap<UpdateAdditionalCostServiceModel, AdditionalCost>().MaxDepth(2);

            // sub contractor (od_scref) - built 2026-08-09 as a prerequisite for AIN
            // (Additional Issue Note), see SubContractor.cs for the full gap history.
            CreateMap<SubContractor, SubContractorServiceModel>().MaxDepth(2);
            CreateMap<CreateSubContractorServiceModel, SubContractor>().MaxDepth(2);
            CreateMap<UpdateSubContractorServiceModel, SubContractor>().MaxDepth(2);

            // stock reference
            CreateMap<Stock, StockServiceModel>().MaxDepth(2);
            CreateMap<CreateStockServiceModel, Stock>().MaxDepth(2);
            CreateMap<UpdateStockServiceModel, Stock>().MaxDepth(2);

            // order item catalog (od_itm - StockCode/ItemCode master). Maps onto the
            // StockItem entity/StockItems table - the catalog Material Consumption's
            // auto-add-on-save, GarmentAdditionalCostService, OrderItemFeatureService,
            // and the GRN/DGN/GTN/SAN/SRN/Supplier Return note services all actually
            // read from, NOT the separate OrderItems table.
            CreateMap<StockItem, OrderItemCatalogServiceModel>().MaxDepth(2);
            CreateMap<CreateOrderItemCatalogServiceModel, StockItem>().MaxDepth(2);
            CreateMap<UpdateOrderItemCatalogServiceModel, StockItem>().MaxDepth(2);

            // PO
            CreateMap<PurchaseOrder, PurchaseOrderServiceModel>().MaxDepth(2)
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Buyer, opt => opt.MapFrom(src => src.Buyer))
                .ForMember(src => src.BasisValue, opt => opt.MapFrom(src => src.BasisValue))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.TotalQuantity, opt => opt.MapFrom(src => src.TotalQuantity))
                .ForMember(src => src.UnitCode, opt => opt.MapFrom(src => src.UnitCode))
                .ForMember(src => src.GarmentType, opt => opt.MapFrom(src => src.GarmentType))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
                .ForMember(src => src.Season, opt => opt.MapFrom(src => src.Season))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ReverseMap();

            CreateMap<CreatePurchaseOrderServiceModel, PurchaseOrder>().MaxDepth(2)
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

            // address 
            CreateMap<AddressServiceModel, Address>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.Default, opt => opt.MapFrom(src => src.Default))
                .ForMember(src => src.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
                .ForMember(src => src.AddressType, opt => opt.MapFrom(src => src.AddressType))
                .ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
                .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ReverseMap();

            CreateMap<CreateAddressServiceModel, Address>()
                .ForMember(dest => dest.AddressId, opt => opt.MapFrom(src => Guid.NewGuid())); // 🚀 Generate new Guid automatically!
                        
            CreateMap<UpdateAddressServiceModel, Address>().MaxDepth(2);

            // Supplier

            CreateMap<SupplierServiceModel, Supplier>().MaxDepth(2)
               .ForMember(src => src.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
             .ReverseMap();

            CreateMap<CreateSupplierServiceModel, Supplier>().MaxDepth(2)
               .ForMember(src => src.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
               .ReverseMap();

            CreateMap<UpdateSupplierServiceModel, Supplier>().MaxDepth(2)
           .ForMember(src => src.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
            .ReverseMap();

            // From Service DTO to ApparelProUser Model
            //CreateMap<RegisterUserServiceModel, ApparelProUser>()
            //    .ForMember(dest => dest.City, opt => opt.Ignore()) // Ignored since city now sits in Address table
            //    .ForMember(dest => dest.Country, opt => opt.Ignore());

            CreateMap<RegisterUserServiceModel, ApparelProUser>().MaxDepth(2)
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                //.ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
                //.ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
                .ReverseMap();


            // user
            // this is used by when register in react apppro
            // adjusted UserName to KnownAs as User
            CreateMap<RegisterUserServiceModel, User>().MaxDepth(2)
                //  .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                //.ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
                //.ForMember(src => src.Bank, opt => opt.MapFrom(src => src.Country))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
                .ReverseMap();

            CreateMap<UserServiceModel, User>().MaxDepth(2)
               .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
               //.ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
               //.ForMember(src => src.Bank, opt => opt.MapFrom(src => src.Country))
               .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
               .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
               .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
               .ReverseMap();

            CreateMap<ApparelProUser, RegisteredUserServiceModel>().MaxDepth(2)
                .ReverseMap();

            CreateMap<ApparelProUser, UserServiceModel>().MaxDepth(2)
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.Address, opt => opt.Ignore())
                .ForMember(src => src.LastActive, opt => opt.Ignore())
                .ForMember(src => src.PasswordSalt, opt => opt.Ignore())
                .ForMember(src => src.PasswordHash, opt => opt.Ignore())
                .ForMember(src => src.Id, opt => opt.Ignore())
              .ReverseMap();

            // map Address => UserServiceModel

            CreateMap<Address, UserServiceModel>().MaxDepth(2)
                .ForMember(src => src.Address, opt => opt.MapFrom(src => src)); // map entire address 


            // CreateMap<UserServiceModel, UserAPIModel>().MaxDepth(2)
            // .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
            //.ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
            //.ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            //.ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
            //.ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
            // .ForMember(src => src.Address, opt => opt.Ignore())
            // .ForMember(src => src.LastActive, opt => opt.Ignore())
            // .ForMember(src => src.PasswordSalt, opt => opt.Ignore())
            // .ForMember(src => src.PasswordHash, opt => opt.Ignore())
            // .ForMember(src => src.Id, opt => opt.Ignore())

            //CreateMap<UpdateUserAPIModel, UpdateUserServiceModel>().MaxDepth(2)

            // currency Exchange
            CreateMap<CreateCurrencyExchangeServiceModel, CurrencyExchange>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
                .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
                .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
                .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
                .ReverseMap();

            CreateMap<CurrencyExchangeServiceModel, CurrencyExchange>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
                .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
                .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
                .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
                .ReverseMap();

            // currency conversion (od_conv-style flat From/To rate table). ServiceModel's
            // FromCurrencyName/ToCurrencyName are joined in from the Currency master by
            // CurrencyConversionService, not by AutoMapper - CurrencyConversion has no such
            // columns, so ReverseMap simply leaves them unmapped going the other way (same as
            // OrderItemCatalogServiceModel.StockDescription vs StockItem below).
            CreateMap<CurrencyConversion, CurrencyConversionServiceModel>().MaxDepth(2).ReverseMap();
            CreateMap<CreateCurrencyConversionServiceModel, CurrencyConversion>().MaxDepth(2);
            CreateMap<UpdateCurrencyConversionServiceModel, CurrencyConversion>().MaxDepth(2);

            // feature
            CreateMap<CreateItemFeatureServiceModel, ItemFeature>().MaxDepth(2)
                .ForMember(src => src.FeatureCode, opt => opt.MapFrom(src => src.FeatureCode))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ReverseMap();

            CreateMap<ItemFeatureServiceModel, ItemFeature>().MaxDepth(2)
               .ForMember(src => src.FeatureCode, opt => opt.MapFrom(src => src.FeatureCode))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ReverseMap();

            // style
            CreateMap<CreateStyleDetailsServiceModel, Style>().MaxDepth(2)
                .ForMember(src => src.ApprovedDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ApprovedDate)))
                .ForMember(src => src.EstimateApprovalDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.EstimateApprovalDate)))
                .ForMember(src => src.ProductionEndDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ProductionEndDate)))
                .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.OrderDate)))                
                .ReverseMap();

            CreateMap<Style, StyleDetailsServiceModel>().MaxDepth(2);

            // color/size details

            CreateMap<ColorSizeDetails, ColorSizeBreakdownDetailsServiceModel>()
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
                .ForMember(src => src.StyleCode, opt => opt.MapFrom(src => src.StyleCode))
                .ForMember(src => src.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(src => src.Size, opt => opt.MapFrom(src => src.Size))
                .ForMember(src => src.Quantity, opt => opt.MapFrom(src => src.Qty))
                .ForMember(src => src.Ratio, opt => opt.MapFrom(src => src.Ratio))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ReverseMap().MaxDepth(2);

            CreateMap<CreateColorSizeBreakdownDetailsServiceModel, ColorSizeDetails>()
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
                .ForMember(src => src.StyleCode, opt => opt.MapFrom(src => src.StyleCode))
                .ForMember(src => src.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(src => src.Size, opt => opt.MapFrom(src => src.Size))
                .ForMember(src => src.Qty, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(src => src.Ratio, opt => opt.MapFrom(src => src.Ratio))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .MaxDepth(2);

            // color quantity ratio (od_clqr equivalent)

            CreateMap<ColorQuantityRatio, ColorQuantityRatioServiceModel>().ReverseMap().MaxDepth(2);
            CreateMap<CreateColorQuantityRatioServiceModel, ColorQuantityRatio>().MaxDepth(2);

            // material consumptions.

            CreateMap<OrderItem, OrderItemServiceModel>().MaxDepth(2);
            CreateMap<OrderItemFeature, OrderItemFeatureServiceModel>()
                .ForMember(src => src.ItemCode, opt => opt.MapFrom(src => src.ItemCode))
                .ForMember(src => src.StockCode, opt => opt.MapFrom(src => src.StockCode))
                .ForMember(src => src.Feature1Label, opt => opt.MapFrom(src => src.Feature1Type))
                .ForMember(src => src.Feature2Label, opt => opt.MapFrom(src => src.Feature2Type))
                .ForMember(src => src.Feature3Label, opt => opt.MapFrom(src => src.Feature3Type))
                .ForMember(src => src.Feature4Label, opt => opt.MapFrom(src => src.Feature4Type))
                .ForMember(src => src.CostPerUnit, opt => opt.MapFrom(src => src.CostPerUnit))
                .MaxDepth(2);

            CreateMap<OrderItemFeature, OrderItemFeatureMappingServiceModel>().MaxDepth(2);
            CreateMap<CreateOrderItemFeatureMappingServiceModel, OrderItemFeature>().MaxDepth(2);

            // orderwise inventory
            //CreateMap<SRNServiceModel,SRN>().MaxDepth(2);

            // report stylewise 
            //CreateMap<StyleApprovalDetailsServiceModel, OrderItemServiceModel>().MaxDepth(2);

            //CreateMap<ColorSizeDetails, StyleDimensionsLookupServiceModel>().MaxDepth(2);

            // permissions (Stage 2 access-control rework)
            CreateMap<Permission, PermissionServiceModel>().MaxDepth(2).ReverseMap();

            // system configuration
            CreateMap<SystemParameter, SystemParameterServiceModel>().MaxDepth(2).ReverseMap();

            // production control
            CreateMap<ProductionLineServiceModel, ProductionLine>().MaxDepth(2).ReverseMap();
            CreateMap<CreateProductionLineServiceModel, ProductionLine>().MaxDepth(2);
            CreateMap<UpdateProductionLineServiceModel, ProductionLine>().MaxDepth(2);

            CreateMap<OperationServiceModel, Operation>().MaxDepth(2).ReverseMap();
            CreateMap<CreateOperationServiceModel, Operation>().MaxDepth(2);
            CreateMap<UpdateOperationServiceModel, Operation>().MaxDepth(2);

            CreateMap<NonProductiveHourCodeServiceModel, NonProductiveHourCode>().MaxDepth(2).ReverseMap();
            CreateMap<CreateNonProductiveHourCodeServiceModel, NonProductiveHourCode>().MaxDepth(2);
            CreateMap<UpdateNonProductiveHourCodeServiceModel, NonProductiveHourCode>().MaxDepth(2);

            CreateMap<MachineTypeServiceModel, MachineType>().MaxDepth(2).ReverseMap();
            CreateMap<CreateMachineTypeServiceModel, MachineType>().MaxDepth(2);
            CreateMap<UpdateMachineTypeServiceModel, MachineType>().MaxDepth(2);

            CreateMap<GarmentComponentServiceModel, GarmentComponent>().MaxDepth(2).ReverseMap();
            CreateMap<CreateGarmentComponentServiceModel, GarmentComponent>().MaxDepth(2);
            CreateMap<UpdateGarmentComponentServiceModel, GarmentComponent>().MaxDepth(2);

            CreateMap<EmployeeServiceModel, Employee>().MaxDepth(2).ReverseMap();
            CreateMap<CreateEmployeeServiceModel, Employee>().MaxDepth(2);
            CreateMap<UpdateEmployeeServiceModel, Employee>().MaxDepth(2);

            CreateMap<StyleComponentBreakdownServiceModel, StyleComponentBreakdown>().MaxDepth(2).ReverseMap();
            CreateMap<CreateStyleComponentBreakdownServiceModel, StyleComponentBreakdown>().MaxDepth(2);

            CreateMap<StyleOperationBreakdownServiceModel, StyleOperationBreakdown>().MaxDepth(2).ReverseMap();
            CreateMap<CreateStyleOperationBreakdownServiceModel, StyleOperationBreakdown>().MaxDepth(2);

            CreateMap<ComponentOperationTemplateServiceModel, ComponentOperationTemplate>().MaxDepth(2).ReverseMap();
            CreateMap<CreateComponentOperationTemplateServiceModel, ComponentOperationTemplate>().MaxDepth(2);
            CreateMap<UpdateComponentOperationTemplateServiceModel, ComponentOperationTemplate>().MaxDepth(2);

            CreateMap<HolidayServiceModel, Holiday>().MaxDepth(2).ReverseMap();
            CreateMap<CreateHolidayServiceModel, Holiday>().MaxDepth(2);

            CreateMap<ProductionLineAllocationServiceModel, ProductionLineAllocation>().MaxDepth(2).ReverseMap();

            CreateMap<EstimatedProductionLineAllocationServiceModel, EstimatedProductionLineAllocation>().MaxDepth(2).ReverseMap();

            CreateMap<DailyProductionTimeTicketEntryServiceModel, DailyProductionTimeTicketEntry>().MaxDepth(2).ReverseMap();

            CreateMap<EstimatedProductionEntryServiceModel, EstimatedProductionEntry>().MaxDepth(2).ReverseMap();
            CreateMap<CreateEstimatedProductionEntryServiceModel, EstimatedProductionEntry>().MaxDepth(2);

            CreateMap<SectionServiceModel, Section>().MaxDepth(2).ReverseMap();
            CreateMap<CreateSectionServiceModel, Section>().MaxDepth(2);
            CreateMap<UpdateSectionServiceModel, Section>().MaxDepth(2);

            CreateMap<DailyProductionEntryServiceModel, DailyProductionEntry>().MaxDepth(2).ReverseMap();
            CreateMap<CreateDailyProductionEntryServiceModel, DailyProductionEntry>().MaxDepth(2);
        }
    }
}
