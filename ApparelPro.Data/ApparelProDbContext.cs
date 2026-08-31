using apparelPro.BusinessLogic.Services.Models.OrderManagement.Stylewise_Events;
using ApparelPro.Data.Configurations.OrderManagement;
using ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Configurations.OrderManagement.Shipment;
using ApparelPro.Data.Configurations.GeneralInventory;
using ApparelPro.Data.Configurations.OrderManagement.Stylewise_events;
using ApparelPro.Data.Configurations.OrderManagement.SubContracting;
using ApparelPro.Data.Configurations.OrderwiseInventory;
using ApparelPro.Data.Configurations.Production;
using ApparelPro.Data.Configurations.References;
using ApparelPro.Data.Configurations.Registration;
using ApparelPro.Data.Configurations.SystemConfiguration;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using ApparelPro.Data.Models.OrderManagement.SubContracting;
using ApparelPro.Data.Models.OrderwiseInventory;
using ApparelPro.Data.Models.GeneralInventory;
using ApparelPro.Data.Models.Production;
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using ApparelPro.Data.Models.SystemConfiguration;
using Microsoft.EntityFrameworkCore;

namespace ApparelPro.Data
{
    public class ApparelProDbContext:DbContext
    {
        // Add this temporary block inside your ApparelProDbContext class:
        public ApparelProDbContext()
        {
        }
        public ApparelProDbContext(DbContextOptions<ApparelProDbContext> options):base(options)
        {
        }

        // this entry is used to bypass program.cs in case of connection issues, instead offline via this
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Points directly to your local SQL Express server using Windows Authentication
                optionsBuilder.UseSqlServer("Server=THUSITHPC\\SQLEXPRESS;Database=ApparelPro;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Buyer> Buyers { get; set; }
        public virtual DbSet<Destination> Destinations { get; set; }
        public virtual DbSet<Bank> Banks { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<GarmentType> GarmentTypes { get; set; }
        public virtual DbSet<Style> Styles { get; set; }
        public virtual DbSet<Basis> Basis { get; set; }
        public virtual DbSet<Season> Seasons { get; set; }
        public virtual DbSet<AdditionalCost> AdditionalCosts { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<UnitConversion> UnitConversion { get; set; }
        public virtual DbSet<Stock> Stocks { get; set; }
        public virtual DbSet<StockItem> StockItems { get; set; }
        public virtual DbSet<SupplierPurchaseOrderDetails>  SupplierPurchaseOrderDetails { get; set; }
        public virtual DbSet<CurrencyExchange> CurrencyExchanges { get; set; }
        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public virtual DbSet<CurrencyConversion> CurrencyConversions { get; set; }

        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<SubContractor> SubContractors { get; set; }

        // Order Management
        public virtual DbSet<ItemFeature> ItemFeatures { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<GarmentTypeItems> GarmentTypeItems { get; set; } = null!;
        public virtual DbSet<OrderItemFeature> OrderItemFeatures { get; set; }
        public virtual DbSet<StyleMaterialConsumptionLedger> StyleMaterialConsumptionLedgers { get; set; }
        public virtual DbSet<StyleMaterialCostProfile> StyleMaterialCostProfiles { get; set; }
        public virtual DbSet<GarmentAdditionalCost> GarmentAdditionalCosts { get; set; }
        public virtual DbSet<SubContract> SubContracts { get; set; }
        public virtual DbSet<ColorSizeDetails> ColorSizeDetails { get; set; }
        public virtual DbSet<ColorQuantityRatio> ColorQuantityRatios { get; set; }

        public virtual DbSet<SupplierPurchaseOrder>  SupplierPurchaseOrders { get; set; }
        public virtual DbSet<EventMaster> EventMasters { get; set; } = null;
        public virtual DbSet<StylewiseEvent> StylewiseEvents { get; set; } = null;

        // inventory
        public virtual DbSet<OrderwiseStock> OrderwiseStocks { get; set; }
        public virtual DbSet<GeneralStockReference> GeneralStockReferences { get; set; }
        public virtual DbSet<OrderwiseStockMaster> OrderwiseStockMasters { get; set; }

        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<OrderwiseStockTransaction> OrderwiseStockTransactions { get; set; } = null!;
        public virtual DbSet<DocumentSequence> DocumentSequences { get; set; } = null!;


        // shipments
        public virtual DbSet<PartShipment> PartShipments { get; set; } = null!;
        public virtual DbSet<QuotaTransaction> QuotaTransactions { get; set; } = null!;
        public virtual DbSet<CommercialInvoiceHeader> CommercialInvoiceHeaders { get; set; } = null!;
        public virtual DbSet<CommercialInvoiceLine> CommercialInvoiceLines { get; set; } = null!;

        // general inventory
        public virtual DbSet<GeneralStockMaster> GeneralStockMasters { get; set; } = null!;
        public virtual DbSet<GeneralStockTransaction> GeneralStockTransactions { get; set; } = null!;
        public virtual DbSet<GeneralPurchaseOrder> GeneralPurchaseOrders { get; set; } = null!;
        public virtual DbSet<GeneralPurchaseOrderDetails> GeneralPurchaseOrderDetails { get; set; } = null!;
        public virtual DbSet<GeneralStore> GeneralStores { get; set; } = null!;

        // system configuration
        public virtual DbSet<SystemParameter> SystemParameters { get; set; } = null!;

        // production control
        public virtual DbSet<ProductionLine> ProductionLines { get; set; } = null!;
        public virtual DbSet<Operation> Operations { get; set; } = null!;
        public virtual DbSet<NonProductiveHourCode> NonProductiveHourCodes { get; set; } = null!;
        public virtual DbSet<MachineType> MachineTypes { get; set; } = null!;
        public virtual DbSet<GarmentComponent> GarmentComponents { get; set; } = null!;
        public virtual DbSet<Employee> Employees { get; set; } = null!;
        public virtual DbSet<StyleComponentBreakdown> StyleComponentBreakdowns { get; set; } = null!;
        public virtual DbSet<StyleOperationBreakdown> StyleOperationBreakdowns { get; set; } = null!;
        public virtual DbSet<ComponentOperationTemplate> ComponentOperationTemplates { get; set; } = null!;
        public virtual DbSet<StyleProductionCapacity> StyleProductionCapacities { get; set; } = null!;
        public virtual DbSet<Holiday> Holidays { get; set; } = null!;
        public virtual DbSet<ProductionLineAllocation> ProductionLineAllocations { get; set; } = null!;
        public virtual DbSet<EstimatedProductionLineAllocation> EstimatedProductionLineAllocations { get; set; } = null!;
        public virtual DbSet<DailyProductionTimeTicketEntry> DailyProductionTimeTicketEntries { get; set; } = null!;
        public virtual DbSet<EstimatedProductionEntry> EstimatedProductionEntries { get; set; } = null!;
        public virtual DbSet<Section> Sections { get; set; } = null!;
        public virtual DbSet<DailyProductionEntry> DailyProductionEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // 1. Force the database engine to maintain a stable, active state on the server
            modelBuilder.HasAnnotation("SqlServer:Identity", "1, 1");

            // Note: EF Core does not have a native fluent API for 'AUTO_CLOSE' or explicit initial file sizes.
            // To handle this cleanly via code-first migrations, we can generate an empty migration 
            // and inject raw T-SQL directly into the migration's Up() method.

            modelBuilder.ApplyConfiguration(new BuyerConfig());
            modelBuilder.ApplyConfiguration(new CurrencyConfig());
            modelBuilder.ApplyConfiguration(new DestinationConfig());
            modelBuilder.ApplyConfiguration(new AddressConfig());
            modelBuilder.ApplyConfiguration(new BankConfig());
            modelBuilder.ApplyConfiguration(new CountryConfig());
            modelBuilder.ApplyConfiguration(new GarmentTypeConfig());
            modelBuilder.ApplyConfiguration(new StyleConfig());
            modelBuilder.ApplyConfiguration(new BasisConfig());
            modelBuilder.ApplyConfiguration(new SeasonConfig());
            modelBuilder.ApplyConfiguration(new AdditionalCostConfig());
            modelBuilder.ApplyConfiguration(new UnitConfig());
            modelBuilder.ApplyConfiguration(new UnitConversionConfig());
            modelBuilder.ApplyConfiguration(new StockConfig());
            modelBuilder.ApplyConfiguration(new StockItemConfig());
            modelBuilder.ApplyConfiguration(new SupplierPurchaseOrderDetailsConfig());
            modelBuilder.ApplyConfiguration(new CurrencyExchangeConfig());
            modelBuilder.ApplyConfiguration(new UserConfig());
            modelBuilder.ApplyConfiguration(new SupplierConfig());
            modelBuilder.ApplyConfiguration(new CurrencyConversionConfig());
            modelBuilder.ApplyConfiguration(new SubContractorConfig());

            // order management
            modelBuilder.ApplyConfiguration(new PurchaseOrderConfig());
            modelBuilder.ApplyConfiguration(new ColorSizeDetailsConfig());
            modelBuilder.ApplyConfiguration(new ColorQuantityRatioConfig());

            modelBuilder.ApplyConfiguration(new ItemFeatureConfig());
            modelBuilder.ApplyConfiguration(new OrderItemConfig());
            modelBuilder.ApplyConfiguration(new GarmentTypeItemsConfig());

            modelBuilder.ApplyConfiguration(new OrderItemFeatureConfig());
            modelBuilder.ApplyConfiguration(new StyleMaterialCostProfileConfig());
            modelBuilder.ApplyConfiguration(new StyleMaterialConsumptionLedgerConfig());
            modelBuilder.ApplyConfiguration(new GarmentAdditionalCostConfig());
            modelBuilder.ApplyConfiguration(new SubContractConfig());
            modelBuilder.ApplyConfiguration(new SupplierPurchaseOrderConfig());

            // styelwise events
            modelBuilder.ApplyConfiguration(new StylewiseEventConfig());
            modelBuilder.ApplyConfiguration(new EventMasterConfig());

            // inventory
            modelBuilder.ApplyConfiguration(new OrderwiseStockConfig());
            modelBuilder.ApplyConfiguration(new GeneralStockReferenceConfig());
            modelBuilder.ApplyConfiguration(new OrderwiseStockMasterConfig());

            modelBuilder.ApplyConfiguration(new DocumentSequenceConfig());
            modelBuilder.ApplyConfiguration(new DepartmentConfig());
            modelBuilder.ApplyConfiguration(new OrderwiseStockTransactionConfig());

            // shipments
            modelBuilder.ApplyConfiguration(new PartShipmentConfig());
            modelBuilder.ApplyConfiguration(new QuotaTransactionConfig());
            modelBuilder.ApplyConfiguration(new CommercialInvoiceHeaderConfig());
            modelBuilder.ApplyConfiguration(new CommercialInvoiceLineConfig());

            // general inventory
            modelBuilder.ApplyConfiguration(new GeneralStockMasterConfig());
            modelBuilder.ApplyConfiguration(new GeneralStockTransactionConfig());
            modelBuilder.ApplyConfiguration(new GeneralPurchaseOrderConfig());
            modelBuilder.ApplyConfiguration(new GeneralPurchaseOrderDetailsConfig());
            modelBuilder.ApplyConfiguration(new GeneralStoreConfig());

            // system configuration
            modelBuilder.ApplyConfiguration(new SystemParameterConfig());

            // production control
            modelBuilder.ApplyConfiguration(new ProductionLineConfig());
            modelBuilder.ApplyConfiguration(new OperationConfig());
            modelBuilder.ApplyConfiguration(new NonProductiveHourCodeConfig());
            modelBuilder.ApplyConfiguration(new MachineTypeConfig());
            modelBuilder.ApplyConfiguration(new GarmentComponentConfig());
            modelBuilder.ApplyConfiguration(new EmployeeConfig());
            modelBuilder.ApplyConfiguration(new StyleComponentBreakdownConfig());
            modelBuilder.ApplyConfiguration(new StyleOperationBreakdownConfig());
            modelBuilder.ApplyConfiguration(new ComponentOperationTemplateConfig());
            modelBuilder.ApplyConfiguration(new StyleProductionCapacityConfig());
            modelBuilder.ApplyConfiguration(new HolidayConfig());
            modelBuilder.ApplyConfiguration(new ProductionLineAllocationConfig());
            modelBuilder.ApplyConfiguration(new EstimatedProductionLineAllocationConfig());
            modelBuilder.ApplyConfiguration(new DailyProductionTimeTicketEntryConfig());
            modelBuilder.ApplyConfiguration(new EstimatedProductionEntryConfig());
            modelBuilder.ApplyConfiguration(new SectionConfig());
            modelBuilder.ApplyConfiguration(new DailyProductionEntryConfig());
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer("ApparelProConnection");
        //}
    }
}
