using apparelPro.BusinessLogic.Services.Implementation.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBasisService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyExchangeService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeService;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using AutoMapper;

namespace apparelPro.BusinessLogic.Services.Mappings
{
    public class DatabaseToServiceMappings : Profile
    {
        public DatabaseToServiceMappings()
        {
            // currency
            CreateMap<Currency, CurrencyServiceModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.CurrencyDetails, opt => opt.MapFrom(src => src.CurrencyDetails))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateCurrencyServiceModel, Currency>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id));

            // country
            CreateMap<Country, CountryServiceModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateCountryServiceModel, Country>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<UpdateCountryServiceModel, Country>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag));

            // currency
            CreateMap<UpdateCurrencyServiceModel, Currency>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForAllMembers(opt => opt.Ignore());

            // Bank
            CreateMap<BankServiceModel, Bank>()
                .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))
                .ForMember(src => src.SwiftCode, opt => opt.MapFrom(src => src.SwiftCode))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
                .ReverseMap();

            // buyer
            CreateMap<Buyer, BuyerServiceModel>()
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.Fax, opt => opt.MapFrom(src => src.Fax))
                .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
                .ForMember(src => src.MobileNos, opt => opt.MapFrom(src => src.MobileNos))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CUSDEC, opt => opt.MapFrom(src => src.CUSDEC))
                .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            // Unit
            CreateMap<Unit, UnitServiceModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateUnitServiceModel, Unit>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            // Garment Type
            CreateMap<GarmentType, GarmentTypeServiceModel>()
                .ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<UpdateGarmentTypeServiceModel, GarmentType>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName))
                .ReverseMap()
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            // basis

            CreateMap<Basis, BasisServiceModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            // PO
            CreateMap<PurchaseOrder, PurchaseOrderServiceModel>()
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

            CreateMap<CreatePurchaseOrderServiceModel, PurchaseOrder>()
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
            CreateMap<AddressServiceModel, Address>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Default, opt => opt.MapFrom(src => src.Default))
                .ForMember(src => src.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.AddressType, opt => opt.MapFrom(src => src.AddressType))
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
                .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ReverseMap();

            CreateMap<CreateAddressServiceModel, Address>();
            CreateMap<UpdateAddressServiceModel, Address>();

            // user

            CreateMap<RegisterUserServiceModel, User>()
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
                .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
                .ReverseMap();

            CreateMap<UserServiceModel, User>()
               .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
               .ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
               .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
               .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
               .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
               .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
               .ReverseMap();

            CreateMap<ApparelProUser, RegisteredUserServiceModel>()           
                .ReverseMap();            

            // currency Exchange
            CreateMap<CreateCurrencyExchangeServiceModel, CurrencyExchange>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
                .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
                .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
                .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
                .ReverseMap();

            CreateMap<CurrencyExchangeServiceModel, CurrencyExchange>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
                .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
                .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
                .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
                .ReverseMap();

            // style
            CreateMap<CreateStyleDetailsServiceModel, Style>()
                .ReverseMap();
            CreateMap<StyleDetailsServiceModel, Style>().ReverseMap()
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
                .ForMember(src => src.StyleCode, opt => opt.MapFrom(src => src.StyleCode))
                .ForMember(src => src.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(src => src.Unit, opt => opt.MapFrom(src => src.Unit))
                .ForMember(src => src.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
                .ForMember(src => src.ProductionEndDate, opt => opt.MapFrom(src => src.ProductionEndDate))
                .ForMember(src => src.EstimateApprovalDate, opt => opt.MapFrom(src => src.EstimateApprovalDate))
                .ForMember(src => src.ApprovedDate, opt => opt.MapFrom(src => src.ApprovedDate));
        }
    }
}
