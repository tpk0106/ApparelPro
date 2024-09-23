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
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using ApparelPro.Shared.Extensions;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels.Registration;
using ApparelPro.WebApi.APIModels.Shared;
using AutoMapper;
namespace ApparelPro.WebApi.Mappings
{
    public class ServicetoAPIModelMappings : Profile
    {
        public ServicetoAPIModelMappings()
        {
            // currency
            CreateMap<CurrencyServiceModel, CurrencyAPIModel>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForMember(src => src.CurrencyDetails, opt => opt.MapFrom(src => src.CurrencyDetails)).ReverseMap()
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CurrencyAPIModel,UpdateCurrencyServiceModel>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))            
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))            
                .ForAllMembers(opt => opt.Ignore());
            
            CreateMap<UpdateCurrencyAPIModel, UpdateCurrencyServiceModel>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor));
                // .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateCurrencyAPIModel, CreateCurrencyServiceModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.Minor, opt => opt.MapFrom(src => src.Minor))
                .ForMember(src => src.CountryCode, opt => opt.MapFrom(src => src.CountryCode));            

            // country
            CreateMap<CreateCountryAPIModel, CreateCountryServiceModel>()
              .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
              .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(src => src.Flag, opt => opt.MapFrom(src => src.Flag));

            CreateMap<UpdateCountryAPIModel, UpdateCountryServiceModel>()
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
            CreateMap<GarmentTypeServiceModel, GarmentTypeAPIModel>()
                .ReverseMap();

            CreateMap<UpdateGarmentTypeAPIModel, UpdateGarmentTypeServiceModel>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.TypeName, opt => opt.MapFrom(src => src.TypeName));


            // unit
            CreateMap<UnitServiceModel, UnitAPIModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<UpdateUnitAPIModel,UpdateUnitServiceModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<CreateUnitAPIModel, CreateUnitServiceModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            // bank
            CreateMap<BankAPIModel, BankServiceModel>()
                .ForMember(src => src.BankCode, opt => opt.MapFrom(src => src.BankCode))        
                .ForMember(src => src.SwiftCode   , opt => opt.MapFrom(src => src.SwiftCode))
                .ForMember(src => src.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(src => src.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(src => src.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(src => src.LoanLimit, opt => opt.MapFrom(src => src.LoanLimit))
                .ReverseMap();

            // basis
            CreateMap<BasisServiceModel, BasisAPIModel>()
                .ForMember(src => src.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(src => src.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            // PO
            CreateMap<PurchaseOrderServiceModel, POAPIModel>()
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
            
            CreateMap<CreatePOAPIModel, CreatePurchaseOrderServiceModel>()
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
            CreateMap<BuyerServiceModel, BuyerAPIModel>()
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

            // address
            CreateMap<AddressServiceModel, AddressAPIModel>()
                .ReverseMap();

            // User
            CreateMap<UserAPIModel, UserServiceModel>()
                .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
                .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.ProfilePhoto))
                .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
                .ReverseMap();

            CreateMap<CreateUserAPIModel, UserServiceModel>()
               .ForMember(src => src.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(src => src.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
               .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
               .ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
               .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country))
               .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
               .ForMember(src => src.Created, opt => opt.MapFrom(src => src.Created))
               .ForMember(src => src.LastActive, opt => opt.MapFrom(src => src.LastActive))
               .ReverseMap();

            CreateMap<RegisterUserAPIModel, RegisterUserServiceModel>()
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(src => src.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.City, opt => opt.MapFrom(src => src.City))
                .ForMember(src => src.Country, opt => opt.MapFrom(src => src.Country));            

            CreateMap<ApparelProUser, RegisteredUserAPIModel>()
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.ProfilePhoto));
                //.ForMember(src => src.Success, opt => opt.MapFrom(src => (src.Token != null && src.Token.Trim().Length > 0)));

            CreateMap<RegisterUserServiceModel, RegisteredUserAPIModel>()
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(src => src.Success, opt => opt.MapFrom(src => (src.Token != null && src.Token.Trim().Length > 0)));

            CreateMap<RegisteredUserServiceModel, RegisteredUserAPIModel>()
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.KnownAs, opt => opt.MapFrom(src => src.KnownAs))
                .ForMember(src => src.Token, opt => opt.MapFrom(src => src.Token))
                .ForMember(src => src.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(src => src.RefreshTokenExpiry, opt => opt.MapFrom(src => src.RefreshTokenExpiry))
                .ForMember(src => src.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(src => src.Success, opt => opt.MapFrom(src => (src.Token != null && src.Token.Trim().Length > 0)));

            CreateMap<LoginUserAPIModel, LoginUserServiceModel>()
                .ForMember(src => src.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(src => src.Password, opt => opt.MapFrom(src => src.Password));

            // currency exchange
            CreateMap<CreateCurrencyExchangeAPIModel, CreateCurrencyExchangeServiceModel>()             
                .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
                .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
                .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
                .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
                .ReverseMap();

            CreateMap<CurrencyExchangeAPIModel, CurrencyExchangeServiceModel>()              
                .ForMember(src => src.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
                .ForMember(src => src.QuoteCurrency, opt => opt.MapFrom(src => src.QuoteCurrency))
                .ForMember(src => src.ExchangeDate, opt => opt.MapFrom(src => src.ExchangeDate))
                .ForMember(src => src.Rate, opt => opt.MapFrom(src => src.Rate))
                .ReverseMap();

            //style
            CreateMap<StyleDetailsServiceModel, StyleAPIModel>().ReverseMap();
            CreateMap<CreateStyleDetailsAPIModel, CreateStyleDetailsServiceModel>()
                .ForMember(src => src.BuyerCode, opt => opt.MapFrom(src => src.BuyerCode))
                .ForMember(src => src.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(src => src.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
                .ForMember(src => src.StyleCode, opt => opt.MapFrom(src => src.StyleCode))
                .ForMember(src => src.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(src => src.Unit, opt => opt.MapFrom(src => src.Unit))
                .ForMember(src => src.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(src => src.OrderDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(src => src.ProductionEndDate, opt => opt.MapFrom(src => DateTime.MinValue))
                .ForMember(src => src.EstimateApprovalDate, opt => opt.MapFrom(src => DateTime.MinValue))
                .ForMember(src => src.ApprovedDate, opt => opt.MapFrom(src => DateTime.MinValue));
                
            CreateMap<UpdateStyleAPIModel,UpdateStyleDetailsServiceModel>();            

            CreateMap(typeof(PaginationResult<>), typeof(PaginationAPIModel<>))
                 .ConvertUsing(typeof(PaginationResultToPaginationAPITypeConverter<,>));

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
