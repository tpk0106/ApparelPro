using apparelPro.BusinessLogic.Services.Implementation.Shared;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeDetailsService;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMaterialConsumptionService;
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
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitConversionService;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using ApparelPro.Shared.Extensions;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels.Registration;
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
            // .ForAllMembers(opt => opt.Ignore());

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

            //CreateMap<CountryServiceModel, CountryAPIModel>()
            //    .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
            //    .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
            //    .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag))
            //    .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
            //    .ReverseMap();

            // garment type
            CreateMap<CreateGarmentTypeAPIModel,  CreateGarmentTypeServiceModel>()
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

            CreateMap<UnitConversionServiceModel,UnitConversionAPIModel>().MaxDepth(2)
                .ForMember(src => src.FromUnit, opt => opt.MapFrom(src => src.FromUnit))
                .ForMember(src => src.ToUnit, opt => opt.MapFrom(src => src.ToUnit))
                .ForMember(src => src.Measure, opt => opt.MapFrom(src => src.Measure))
                .MaxDepth(2);


            // bank

            // 1. Map individual address entry structures
            CreateMap<CreateAddressAPIModel, CreateAddressServiceModel>();
            //CreateMap<CreateAddressServiceModel, Address>()
            //    .ForMember(dest => dest.AddressId, opt => opt.MapFrom(src => Guid.NewGuid())); // 🚀 Generate new Guid automatically!

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
               // .ReverseMap();

            CreateMap<CreateBankServiceModel, Bank>();

            CreateMap<UpdateBankAPIModel, UpdateBankServiceModel>().MaxDepth(2)
              .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))
              .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(src => src.TelephoneNos, opt => opt.MapFrom(src => src.TelephoneNos))
              .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
              .ForMember(src => src.SwiftCode, opt => opt.MapFrom(src => src.SwiftCode))
              .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
              .ForMember(src => src.Addresses, opt => opt.MapFrom(src => src.Addresses));
            // .ReverseMap();



            // basis

            CreateMap<UpdateBasisServiceModel, BasisAPIModel>().MaxDepth(2);
            CreateMap<BasisServiceModel, BasisAPIModel>().MaxDepth(2)
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<CreateBasisServiceModel, CreateBasisAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<BasisServiceModel, CreateBasisAPIModel>().MaxDepth(2).ReverseMap();
            CreateMap<UpdateBasisAPIModel, UpdateBasisServiceModel>().MaxDepth(2);

            // destination
            CreateMap<PortDestinationAPIModel, PortDestinationServiceModel>().MaxDepth(2)
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<CreatePortDestinationAPIModel, CreatePortDestinationServiceModel>().MaxDepth(2)
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<CreatePortDestinationServiceModel, Destination>().MaxDepth(2)
              .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
              .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
              .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
              .ReverseMap();

            CreateMap<UpdatePortDestinationAPIModel, UpdatePortDestinationServiceModel>().MaxDepth(2)
              .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
              .ForMember(src => src.DestinationName, opt => opt.MapFrom(src => src.DestinationName))
              .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id));
            //.ReverseMap();

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

            // Feature

            CreateMap<FeatureServiceModel, FeatureAPIModel>().MaxDepth(2)
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                 .ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateFeatureAPIModel, CreateFeatureServiceModel>().MaxDepth(2)
                    .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description));

            CreateMap<UpdateFeatureAPIModel, UpdateFeatureServiceModel>().MaxDepth(2)
                      .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ReverseMap()
              .ForAllMembers(opt => opt.Ignore());


            // User
            //CreateMap<User, UserAPIModel>()
            //CreateMap<UserAPIModel, UserServiceModel>().MaxDepth(2)
            //    //  .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
            //    .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
            //    .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
            //    .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            //    .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
            //    .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
            //    .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.ProfilePhoto))
            //    .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
            //    .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive));
            //.ReverseMap();

            CreateMap<UserServiceModel, UserAPIModel>()
                .MaxDepth(2)
               .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
               .ForMember(src => src.Address, opt => opt.MapFrom(src => src.Address))
               .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.Address.AddressId))
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
               //.ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
               //.ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
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
               .ForPath(dest => dest.Address.StreetAddress, opt => opt.MapFrom(src => src.Address.StreetAddress))               
               .ForPath(dest => dest.Address.City, opt => opt.MapFrom(src => src.Address.City))
               .ForPath(dest => dest.Address.PostCode, opt => opt.MapFrom(src => src.Address.PostCode))
               .ForPath(dest => dest.Address.State, opt => opt.MapFrom(src => src.Address.State))
               .ForPath(dest => dest.Address.CountryCode, opt => opt.MapFrom(src => src.Address.CountryCode))
               .ForPath(dest => dest.Address.AddressId, opt => opt.MapFrom(src => src.Address.AddressId))
               .ForPath(dest => dest.Address.Default, opt => opt.MapFrom(src => src.Address.Default))
               .ForPath(dest => dest.Address.AddressType, opt => opt.MapFrom(src => src.Address.AddressType));
            

            //CreateMap<UserServiceModel, UpdateUserServiceModel>().MaxDepth(2).ReverseMap();

            // From API DTO to Service DTO
            CreateMap<RegisterUserAPIModel, RegisterUserServiceModel>().MaxDepth(2); // same as below mappings

            //CreateMap<RegisterUserAPIModel, RegisterUserServiceModel>()
            //    .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
            //    //     .ForMember(src => src.Password, opt => opt.MapFrom(src => src.Password)) // added by thusith on 16/02/25
            //    .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            //    .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
            //    .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
            //    .ForMember(src => src.PhoneNumber, opt => opt.MapFrom(src => src.phoneNumber));


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
            // .ReverseMap();

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

            //style
            CreateMap<StyleDetailsServiceModel, StyleAPIModel>().MaxDepth(2)
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
                .ForMember(src => src.StyleCode, opt => opt.MapFrom(src => src.StyleCode))
                .ForMember(src => src.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(src => src.Unit, opt => opt.MapFrom(src => src.Unit))
                .ForMember(src => src.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
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
                //.ForMember(src => src.ProductionEndDate, opt => opt.MapFrom(src => DateTime.MinValue))
                //.ForMember(src => src.EstimateApprovalDate, opt => opt.MapFrom(src => DateTime.MinValue))
                //.ForMember(src => src.ApprovedDate, opt => opt.MapFrom(src => DateTime.MinValue));

            CreateMap<StyleAPIModel, UpdateStyleDetailsServiceModel>().MaxDepth(2);

            CreateMap<UpdateStyleAPIModel, UpdateStyleDetailsServiceModel>().MaxDepth(2);

            CreateMap(typeof(PaginationResult<>), typeof(PaginationAPIModel<>)).MaxDepth(2)
                 .ConvertUsing(typeof(PaginationResultToPaginationAPITypeConverter<,>));

        
            // color/size details
            CreateMap<CreateColorSizeBreakdownDetailsAPIModel, CreateColorSizeBreakdownDetailsServiceModel>()
                .MaxDepth(2);
            CreateMap<ColorSizeBreakdownDetailsServiceModel, ColorSizeBreakdownDetailsAPIModel>()
                .MaxDepth(2);

            // material consumption

            CreateMap<CreateMaterialConsumptionEntryRequestAPIModel, CreateMaterialConsumptionEntryRequestServiceModel>().MaxDepth(2);
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

            CreateMap<StyleDimensionsLookupServiceModel,  StyleDimensionsLookupAPIModel>().MaxDepth(2);
            CreateMap<SupplierLookupServiceModel, SupplierLookupAPIModel>().MaxDepth(2);

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
