using apparelPro.BusinessLogic.Configuration;
using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.OrderManagement;
using apparelPro.BusinessLogic.Services.Implementation.Reference;
using apparelPro.BusinessLogic.Services.Implementation.Registration;
using apparelPro.BusinessLogic.Services.Implementation.Shared;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using ApparelPro.Data;
using ApparelPro.Shared.LookupConstants;
using ApparelPro.Shared.LookupConstants.ApparelProContext;
using ApparelPro.WebApi.Misc;
using Microsoft.EntityFrameworkCore;
using static ApparelPro.WebApi.Mappings.ServicetoAPIModelMappings;

namespace ApparelPro.WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureApparelProDatabase(IServiceCollection services, IConfiguration configuration)        
        {           
            var migrationAssemblyName = typeof(ApparelProDbContext).Assembly.GetName().Name;
            services.AddDbContext<ApparelProDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("ApparelProConnection"),
                action => action.MigrationsAssembly(migrationAssemblyName))
                                .EnableSensitiveDataLogging(true)
                                .EnableDetailedErrors() 
                                //Ensure that sensitive data logging is wrapped in a check so it only activates in the Development environment.
                                );
            //services.AddDbContextPool<ApparelProDbContext>(options => { options.EnableSensitiveDataLogging(); });
        }

        public static void ConfigureApparelProIdentity(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserIdentityDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("ApparelProConnection"),
                    action => action.MigrationsAssembly("ApparelPro.Data")
                );
            });
        }

        public static void ConfigureParamsData(IConfiguration configuration)
        {
            ApplicationParams.PageSize = configuration.GetValue<int>("PageSize");
        }

        public static void ConfigureApparelProServices(IServiceCollection services)
        {
            // reference services
            services.AddTransient<ICurrencyService, CurrencyService>();
            services.AddTransient<ILookupConstants, LookupConstants>();
            services.AddTransient<ICountryService, CountryService>();            
            services.AddTransient(typeof(IUnitServiceT<UnitServiceModel>), typeof(UnitServiceT));
            services.AddTransient(typeof(IUnitService), typeof(UnitService));
            services.AddTransient(typeof(IUserService), typeof(UserService));
            services.AddTransient(typeof(ISecurityService), typeof(SecurityService));
            services.AddTransient(typeof(ICurrencyExchangeService), typeof(CurrencyExchangeService));
            services.AddTransient(typeof(IGarmentTypeService), typeof(GarmentTypeService));
            services.AddTransient(typeof(IBuyerService), typeof(BuyerService));
            services.AddTransient(typeof(IBasisService), typeof(BasisService));
            services.AddTransient(typeof(IBankService), typeof(BankService));
            services.AddTransient(typeof(IAddressService), typeof(AddressService));
            services.AddTransient(typeof(ISupplierService), typeof(SupplierService));
            services.AddTransient(typeof(IPortDestinationService), typeof(PortDestinationService));
            services.AddTransient(typeof(IFeatureService), typeof(FeatureService));
            services.AddTransient(typeof(IUnitConversionService), typeof(UnitConversionService));

            services.AddTransient(typeof(PaginationResultToPaginationAPITypeConverter<,>));
            //  services.AddTransient<IUnitServiceT<UnitServiceModel>>(x=> x.GetRequiredService<IUnitServiceT<UnitServiceModel>>());
        }

        public static void ConfigureApparelProOrderManagementServices(IServiceCollection services)
        {
            // order management srevices
            services.AddTransient(typeof(IColorSizeBreakdownDetailsService), typeof(ColorSizeBreakdownDetailsService));
            services.AddTransient<IPurchaseOrderService, PurchaseOrderService>();            
            services.AddTransient(typeof(IStyleDetailsService),typeof(StyleDetailsService));

            // material consumption
            services.AddTransient(typeof (IMaterialConsumptionService),typeof(MaterialConsumptionService));
        }

        public static void ConfgureAppsettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        }

        public static void UpdateMvcOptions(this IServiceCollection services)
        {
            services.AddMvc(options => options.SuppressAsyncSuffixInActionNames = false);
        }
        public static void AddAuthorization(IServiceCollection services, IConfiguration configuration)
        {
            AuthorizationConfig config = new();
            config.Inject(configuration);
            //AuthorizationConfig.Inject(configuration);
            services.AddAuthorization(AuthorizationConfig.GetAuthroizationOptions);
        }

        //public static void AddAuthorization(this IServiceCollection services, Action<AuthorizationOptions, IConfiguration> getAuthroizationOptions)
        //{
        //    services.AddAuthorizationBuilder().Services.AddAuthorization(getAuthroizationOptions);

        //}
    }
}
