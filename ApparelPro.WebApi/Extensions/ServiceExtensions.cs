using apparelPro.BusinessLogic.Configuration;
using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.OrderManagement;
using apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Implementation.GeneralInventory;
using apparelPro.BusinessLogic.Services.Implementation.Dashboard;
using apparelPro.BusinessLogic.Services.Implementation.Production;
using apparelPro.BusinessLogic.Services.Implementation.Reference;
using apparelPro.BusinessLogic.Services.Implementation.Registration;
using apparelPro.BusinessLogic.Services.Implementation.Shared;
using apparelPro.BusinessLogic.Services.Implementation.SystemConfiguration;
using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.interfaces.Reports.OrderManagement;
using apparelPro.BusinessLogic.Services.Implementation.Reports.OrderManagement;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Implementation.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using apparelPro.BusinessLogic.Services.Reports.Interfaces;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.LookupConstants;
using ApparelPro.Shared.LookupConstants.ApparelProContext;
using ApparelPro.WebApi.Authorization;
using ApparelPro.WebApi.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Serilog;
using static ApparelPro.WebApi.Mappings.ServicetoAPIModelMappings;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ApparelPro.WebApi.Extensions
{
    public static class ServiceExtensions
    {

        // Logging Slow Queries with Serilog

        //You can't optimize what you can't measure.EF Core has built-in logging support that integrates with.NET's ILogger infrastructure.
        //Combined with Serilog, you get structured logs with all the query details you need to find slow paths in production.

        //  The recommended pattern: configure a minimum command execution time so you only log queries that actually
        //  cross a performance budget -- not every query in your application.

        public static void ConfigureApparelProDatabase(IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            var migrationAssemblyName = typeof(ApparelProDbContext).Assembly.GetName().Name;
            //In ASP.NET Core, register your context as scoped (the default):
            services.AddDbContext<ApparelProDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("ApparelProConnection"),
                    action => action.MigrationsAssembly(migrationAssemblyName))
                    .UseLoggerFactory(LoggerFactory.Create(lb => lb.AddSerilog()));

                if (isDevelopment)
                {
                    // ⚠️ Only in development -- logs actual parameter values including PII
                    options.EnableSensitiveDataLogging(true)
                           .EnableDetailedErrors();
                }
            });
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
            services.AddTransient<IAdditionalCostService, AdditionalCostService>();
            services.AddTransient<ISubContractorService, SubContractorService>();
            services.AddTransient<IStockService, StockService>();
            services.AddTransient<IOrderItemCatalogService, OrderItemCatalogService>();
            services.AddTransient(typeof(IUnitServiceT<UnitServiceModel>), typeof(UnitServiceT));
            services.AddTransient(typeof(IUnitService), typeof(UnitService));
            services.AddTransient(typeof(IUserService), typeof(UserService));
            services.AddTransient(typeof(IGroupService), typeof(GroupService));
            services.AddTransient(typeof(IPermissionService), typeof(PermissionService));
            services.AddTransient(typeof(ISecurityService), typeof(SecurityService));
            services.AddTransient(typeof(ICurrencyExchangeService), typeof(CurrencyExchangeService));
            services.AddTransient(typeof(IGarmentTypeService), typeof(GarmentTypeService));
            services.AddTransient<IGarmentTypeItemsService, GarmentTypeItemsService>();
            services.AddTransient(typeof(IBuyerService), typeof(BuyerService));
            services.AddTransient(typeof(IBasisService), typeof(BasisService));
            services.AddTransient(typeof(ISeasonService), typeof(SeasonService));
            services.AddTransient(typeof(IBankService), typeof(BankService));
            services.AddTransient(typeof(IAddressService), typeof(AddressService));
            services.AddTransient(typeof(ISupplierService), typeof(SupplierService));
            services.AddTransient(typeof(IPortDestinationService), typeof(PortDestinationService));
            services.AddTransient(typeof(IItemFeatureService), typeof(ItemFeatureService));
            services.AddTransient(typeof(IOrderItemFeatureService), typeof(OrderItemFeatureService));
            services.AddTransient(typeof(IUnitConversionService), typeof(UnitConversionService));
            services.AddTransient(typeof(IDepartmentService), typeof(DepartmentService));
            services.AddTransient(typeof(ISystemParameterService), typeof(SystemParameterService));
            // FIXED (2026-08-07): ICurrencyConversionService was never registered here at all -
            // CurrencyConversionController has injected it since it was written, so every one of
            // its endpoints (the Currency Conversion reference-data screen) would have thrown a
            // DI resolution failure at request time. Found while wiring ConvertAsync (new this
            // session) into the Trim Sheet Report, which depends on this service actually working.
            services.AddTransient<ICurrencyConversionService, CurrencyConversionService>();

            services.AddTransient(typeof(PaginationResultToPaginationAPITypeConverter<,>));
            //  services.AddTransient<IUnitServiceT<UnitServiceModel>>(x=> x.GetRequiredService<IUnitServiceT<UnitServiceModel>>());
        }

        public static void ConfigureApparelProOrderManagementServices(IServiceCollection services)
        {
            // order management srevices
            services.AddTransient(typeof(IColorSizeBreakdownDetailsService), typeof(ColorSizeBreakdownDetailsService));
            services.AddTransient(typeof(IColorQuantityRatioService), typeof(ColorQuantityRatioService));
            services.AddTransient<IPurchaseOrderService, PurchaseOrderService>();
            services.AddTransient(typeof(IStyleDetailsService),typeof(StyleDetailsService));

            // material consumption
            services.AddTransient(typeof (IMaterialConsumptionService),typeof(MaterialConsumptionService));
            services.AddTransient<IGarmentAdditionalCostService, GarmentAdditionalCostService>();
            services.AddTransient<ISubContractService, SubContractService>();
            services.AddTransient(typeof(ISupplierPurchaseOrderService), typeof(SupplierPurchaseOrderService));

            // Register the Style-wise critical path tracking service loop lifecycle handler
            services.AddScoped<IStylewiseEventService, StylewiseEventService>();
            //services.AddScoped<IStylewiseReportService, StylewiseReportService>();

            // Register the Scheduled Part Shipments logistics transaction engine loop handler
            services.AddScoped<IPartShipmentService, PartShipmentService>();

            // events approval
            // Register the Critical Path Manager Validation Lock service interface mapping
            services.AddScoped<IStyleApprovalService, StyleApprovalService>();

            // Trim Sheet Report (Reports -> Order Management -> Trim Sheet)
            services.AddScoped<ITrimSheetReportService, TrimSheetReportService>();

            // Order Detail Report (Reports -> Order Management -> Order Detail)
            services.AddScoped<IOrderDetailReportService, OrderDetailReportService>();

            // Colour/Size Report (Reports -> Order Management -> Colour/Size)
            services.AddScoped<IColorSizeReportService, ColorSizeReportService>();
            services.AddScoped<IScheduledShipmentsReportService, ScheduledShipmentsReportService>();

            // Shipment Status Report (Reports -> Order Management -> Shipment status Report)
            services.AddScoped<IShipmentStatusReportService, ShipmentStatusReportService>();

            // Year/Season Wise Orders Report (Reports -> Order Management -> Year/Season Wise Orders)
            services.AddScoped<IYearSeasonOrdersReportService, YearSeasonOrdersReportService>();

            // Pending Events Report (Reports -> Order Management -> Pending Events)
            services.AddScoped<IPendingEventsReportService, PendingEventsReportService>();

            // Stock Arrival Status Report (Reports -> Order Management -> Stock Arrival Status)
            services.AddScoped<IStockArrivalStatusReportService, StockArrivalStatusReportService>();

            // Cost of Production Report (Reports -> Order Management -> Cost of Production)
            services.AddScoped<ICostOfProductionReportService, CostOfProductionReportService>();

            // Order/Quota Detail Report (Reports -> Order Management -> Order/Quota Detail)
            services.AddScoped<IOrderQuotaDetailReportService, OrderQuotaDetailReportService>();

            // Post Order Cost Sheet Report (Reports -> Order Management -> Post Order Cost Sheet)
            services.AddScoped<IPostOrderCostSheetReportService, PostOrderCostSheetReportService>();

            // Monthly Actual Shipments Report (Reports -> Order Management -> Monthly Actual Shipments)
            services.AddScoped<IMonthlyActualShipmentsReportService, MonthlyActualShipmentsReportService>();

            // Purchase Order List Report (Reports -> Order Management -> List of P/O's)
            services.AddScoped<IPurchaseOrderListReportService, PurchaseOrderListReportService>();

            // Outstanding Purchase Order List Report (Reports -> Order Management -> List of Outstanding P/O's)
            services.AddScoped<IOutstandingPurchaseOrderListReportService, OutstandingPurchaseOrderListReportService>();

            // orderwise inventory
            services.AddScoped<IStoresRequisitionService, StoresRequisitionService>();
            services.AddScoped<IGoodsIssueService, GoodsIssueService>();
            services.AddScoped<IGoodsReceivedNoteService, GoodsReceivedNoteService>();
            services.AddScoped<IGoodsReturnNoteService, GoodsReturnNoteService>();
            services.AddScoped<IGoodsTransferNoteService, GoodsTransferNoteService>();
            services.AddScoped<IDirectTransferNoteService, DirectTransferNoteService>();
            services.AddScoped<ISupplierReturnNoteService, SupplierReturnNoteService>();
            services.AddScoped<IDamagedGoodsNoteService, DamagedGoodsNoteService>();
            services.AddScoped<IStockAdjustmentNoteService, StockAdjustmentNoteService>();
            services.AddScoped<IAdditionalIssueNoteService, AdditionalIssueNoteService>();
            services.AddScoped<IAdditionalGoodsReceiptNoteService, AdditionalGoodsReceiptNoteService>();
            services.AddScoped<IStockValuationReportService, StockValuationReportService>();
            services.AddScoped<IStockValuationMonthlyReportService, StockValuationMonthlyReportService>();
            services.AddScoped<IStockStatusReportService, StockStatusReportService>();
            services.AddScoped<ITransactionListReportService, TransactionListReportService>();
            services.AddScoped<IItemWiseStockBalanceService, ItemWiseStockBalanceService>();
            services.AddScoped<IRawMaterialControlSheetService, RawMaterialControlSheetService>();
            services.AddScoped<IStockSummaryReportService, StockSummaryReportService>();
            services.AddScoped<IGrnListingReportService, GrnListingReportService>();
            services.AddScoped<IStockMovementReportService, StockMovementReportService>();
            services.AddScoped<IStockMovementItemReportService, StockMovementItemReportService>();

            // general inventory
            services.AddScoped<IGeneralStoresRequisitionService, GeneralStoresRequisitionService>();
            services.AddScoped<IGeneralGoodsIssueService, GeneralGoodsIssueService>();
            services.AddScoped<IGeneralGoodsReceivedService, GeneralGoodsReceivedService>();
            services.AddScoped<IGeneralGoodsTransferService, GeneralGoodsTransferService>();
            services.AddScoped<IOrderGoodsTransferService, OrderGoodsTransferService>();
            services.AddScoped<IGeneralGoodsReturnService, GeneralGoodsReturnService>();
            services.AddScoped<IGeneralDamagedGoodsService, GeneralDamagedGoodsService>();
            services.AddScoped<IGeneralSupplierReturnService, GeneralSupplierReturnService>();
            services.AddScoped<IGeneralPurchaseOrderService, GeneralPurchaseOrderService>();
            services.AddScoped<IGeneralStockMasterService, GeneralStockMasterService>();
            services.AddScoped<IGeneralStockAdjustmentService, GeneralStockAdjustmentService>();
            services.AddScoped<IGeneralStockStatusReportService, GeneralStockStatusReportService>();
            services.AddScoped<IGeneralStockMovementReportService, GeneralStockMovementReportService>();
            services.AddScoped<IGeneralStockValuationReportService, GeneralStockValuationReportService>();
            services.AddScoped<IGeneralStockReorderReportService, GeneralStockReorderReportService>();
            services.AddScoped<IGeneralTransactionListReportService, GeneralTransactionListReportService>();
            services.AddScoped<IGeneralPurchaseOrderListReportService, GeneralPurchaseOrderListReportService>();
            services.AddScoped<IGeneralStockSummaryReportService, GeneralStockSummaryReportService>();
            services.AddScoped<IGeneralGrnListingReportService, GeneralGrnListingReportService>();

            // production control
            services.AddScoped<IProductionLineService, ProductionLineService>();
            services.AddScoped<IOperationService, OperationService>();
            services.AddScoped<INonProductiveHourCodeService, NonProductiveHourCodeService>();
            services.AddScoped<IMachineTypeService, MachineTypeService>();
            services.AddScoped<IGarmentComponentService, GarmentComponentService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IStyleComponentBreakdownService, StyleComponentBreakdownService>();
            services.AddScoped<IStyleOperationBreakdownService, StyleOperationBreakdownService>();
            services.AddScoped<IComponentOperationTemplateService, ComponentOperationTemplateService>();
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IProductionLineAllocationService, ProductionLineAllocationService>();
            services.AddScoped<IEstimatedProductionLineAllocationService, EstimatedProductionLineAllocationService>();
            services.AddScoped<IDailyProductionTimeTicketService, DailyProductionTimeTicketService>();
            services.AddScoped<IProductionProgressGraphService, ProductionProgressGraphService>();
            services.AddScoped<IEndOfProductionConfirmationService, EndOfProductionConfirmationService>();
            services.AddScoped<IEstimatedProductionEntryService, EstimatedProductionEntryService>();
            services.AddScoped<IDailyProductionEntryService, DailyProductionEntryService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ISystemParameterLookupService, SystemParameterLookupService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IToolbarService, apparelPro.BusinessLogic.Services.Implementation.Toolbar.ToolbarService>();
            services.AddScoped<ICompanyAddressService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.CompanyAddressService>();
            services.AddScoped<ICertificateOfOriginService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.CertificateOfOriginService>();
            services.AddScoped<ILetterOfCreditService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.LetterOfCreditService>();
            services.AddScoped<ICustomsDeclarationService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.CustomsDeclarationService>();
            services.AddScoped<ILetterOfCreditCoveringLetterService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.LetterOfCreditCoveringLetterService>();
            services.AddScoped<ICommercialInvoiceService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.CommercialInvoiceService>();
            services.AddScoped<IClearanceOfficeService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.ClearanceOfficeService>();
            services.AddScoped<IPaymentTermService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.PaymentTermService>();
            services.AddScoped<ITransportModeService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.TransportModeService>();
            services.AddScoped<IDutyTaxCodeService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.DutyTaxCodeService>();
            services.AddScoped<ITaxBaseCodeService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.TaxBaseCodeService>();
            services.AddScoped<IAgreementCodeService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.AgreementCodeService>();
            services.AddScoped<ICommodityCodeService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.CommodityCodeService>();
            services.AddScoped<ICustomsProcedureCodeService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.CustomsProcedureCodeService>();
            services.AddScoped<IDocumentTypeService, apparelPro.BusinessLogic.Services.Implementation.ImportExport.DocumentTypeService>();
            services.AddScoped<IProductionSummaryDailyReportService, ProductionSummaryDailyReportService>();
            services.AddScoped<IProductionScheduleReportService, ProductionScheduleReportService>();
            services.AddScoped<IProductionSummaryMonthlyReportService, ProductionSummaryMonthlyReportService>();
            services.AddScoped<IProductionSummaryMonthlyOverviewReportService, ProductionSummaryMonthlyOverviewReportService>();
            services.AddScoped<IProductionSummaryStyleWiseReportService, ProductionSummaryStyleWiseReportService>();
            services.AddScoped<IProductionSummaryStyleWiseDetailedReportService, ProductionSummaryStyleWiseDetailedReportService>();
            services.AddScoped<ILineProductionSummaryReportService, LineProductionSummaryReportService>();
            services.AddScoped<IOperationBreakdownReportService, OperationBreakdownReportService>();
            services.AddScoped<IManpowerRequirementReportService, ManpowerRequirementReportService>();
            services.AddScoped<IDailyEmployeeEfficiencyReportService, DailyEmployeeEfficiencyReportService>();
            services.AddScoped<IMonthlyEmployeeEfficiencyReportService, MonthlyEmployeeEfficiencyReportService>();
            services.AddScoped<ILineEfficiencyReportService, LineEfficiencyReportService>();
            services.AddScoped<IEstimatedProductionScheduleReportService, EstimatedProductionScheduleReportService>();
            services.AddScoped<IProductionAnalysisSummaryReportService, ProductionAnalysisSummaryReportService>();

            // shared service
            services.AddScoped<ISharedService, SharedService>();
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

        public static void ConfigurePermissionAuthorizationInfrastructure(IServiceCollection services)
        {
            // Stage 2 access-control groundwork: dynamic, RolePermissions-table-backed
            // authorization policies (see ApparelPro.WebApi/Authorization/). Registered
            // AFTER AddAuthorization() so these override the default
            // IAuthorizationPolicyProvider/IAuthorizationHandler that AddAuthorization()
            // adds via TryAddSingleton. No controller uses this yet - the cutover from
            // raw [Authorize(Roles = ...)] to [Authorize(Policy = ...)] is a deliberately
            // separate, later pass.
            services.AddMemoryCache();
            services.AddSingleton<IRolePermissionCache, RolePermissionCache>();
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }
    }
}
