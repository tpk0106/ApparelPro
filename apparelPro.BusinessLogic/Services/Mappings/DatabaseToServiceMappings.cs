using apparelPro.BusinessLogic.Services.Implementation.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBasisService;
using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyExchangeService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService;
using apparelPro.BusinessLogic.Services.Models.Reference.IFeatureService;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeService;
using apparelPro.BusinessLogic.Services.Models.Reference.IPortDestinationService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
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
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
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
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            // basis

            CreateMap<Basis, BasisServiceModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

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

            // CreateMap<CreateAddressServiceModel, Address>().MaxDepth(2);
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

            // feature
            CreateMap<CreateFeatureServiceModel, Feature>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ReverseMap();

            CreateMap<FeatureServiceModel, Feature>().MaxDepth(2)
               .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ReverseMap();

            // style
            CreateMap<CreateStyleDetailsServiceModel, Style>().MaxDepth(2)
                .ReverseMap();
            CreateMap<StyleDetailsServiceModel, Style>().MaxDepth(2)
                .ReverseMap()
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
