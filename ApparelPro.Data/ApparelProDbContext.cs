using apparelPro.BusinessLogic.Services.Models.OrderManagement.Stylewise_Events;
using ApparelPro.Data.Configurations.OrderManagement;
using ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Configurations.OrderManagement.Shipment;
using ApparelPro.Data.Configurations.OrderManagement.Stylewise_events;
using ApparelPro.Data.Configurations.OrderManagement.SubContracting;
using ApparelPro.Data.Configurations.OrderwiseInventory;
using ApparelPro.Data.Configurations.References;
using ApparelPro.Data.Configurations.Registration;
using ApparelPro.Data.Configurations.SystemConfiguration;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using ApparelPro.Data.Models.OrderManagement.SubContracting;
using ApparelPro.Data.Models.OrderwiseInventory;
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
        public virtual DbSet<AdditionalCost> AdditionalCosts { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<UnitConversion> UnitConversion { get; set; }
        public virtual DbSet<Stock> Stocks { get; set; }
        public virtual DbSet<StockItem> StockItems { get; set; }
        public virtual DbSet<PODetails>  PODetails { get; set; }
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

        public virtual DbSet<PurchaseOrderHeader>  PurchaseOrderHeaders { get; set; }
        public virtual DbSet<EventMaster> EventMasters { get; set; } = null;
        public virtual DbSet<StylewiseEvent> StylewiseEvents { get; set; } = null;

        // inventory
        public virtual DbSet<OrderwiseStock> OrderwiseStocks { get; set; }
        public virtual DbSet<OrderwiseStockMaster> OrderwiseStockMasters { get; set; }

        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<OrderwiseStockTransaction> OrderwiseStockTransactions { get; set; } = null!;
        public virtual DbSet<DocumentSequence> DocumentSequences { get; set; } = null!;


        // shipments
        public virtual DbSet<PartShipment> PartShipments { get; set; } = null!;
        public virtual DbSet<QuotaTransaction> QuotaTransactions { get; set; } = null!;

        // system configuration
        public virtual DbSet<SystemParameter> SystemParameters { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BuyerConfig());
            modelBuilder.ApplyConfiguration(new CurrencyConfig());
            modelBuilder.ApplyConfiguration(new DestinationConfig());
            modelBuilder.ApplyConfiguration(new AddressConfig());
            modelBuilder.ApplyConfiguration(new BankConfig());
            modelBuilder.ApplyConfiguration(new CountryConfig());
            modelBuilder.ApplyConfiguration(new GarmentTypeConfig());
            modelBuilder.ApplyConfiguration(new StyleConfig());
            modelBuilder.ApplyConfiguration(new BasisConfig());
            modelBuilder.ApplyConfiguration(new AdditionalCostConfig());
            modelBuilder.ApplyConfiguration(new UnitConfig());
            modelBuilder.ApplyConfiguration(new UnitConversionConfig());
            modelBuilder.ApplyConfiguration(new StockConfig());
            modelBuilder.ApplyConfiguration(new StockItemConfig());
            modelBuilder.ApplyConfiguration(new PODetailsConfig());
            modelBuilder.ApplyConfiguration(new CurrencyExchangeConfig());
            modelBuilder.ApplyConfiguration(new UserConfig());
            modelBuilder.ApplyConfiguration(new SupplierConfig());
            modelBuilder.ApplyConfiguration(new CurrencyConversionConfig());
            modelBuilder.ApplyConfiguration(new SubContractorConfig());

            // order management
            modelBuilder.ApplyConfiguration(new PurchaseOrderConfig());
            modelBuilder.ApplyConfiguration(new ColorSizeDetailsConfig());

            modelBuilder.ApplyConfiguration(new ItemFeatureConfig());
            modelBuilder.ApplyConfiguration(new OrderItemConfig());
            modelBuilder.ApplyConfiguration(new GarmentTypeItemsConfig());

            modelBuilder.ApplyConfiguration(new OrderItemFeatureConfig());
            modelBuilder.ApplyConfiguration(new StyleMaterialCostProfileConfig());
            modelBuilder.ApplyConfiguration(new StyleMaterialConsumptionLedgerConfig());
            modelBuilder.ApplyConfiguration(new GarmentAdditionalCostConfig());
            modelBuilder.ApplyConfiguration(new SubContractConfig());
            modelBuilder.ApplyConfiguration(new PurchaseOrderHeaderConfig());

            // styelwise events
            modelBuilder.ApplyConfiguration(new StylewiseEventConfig());
            modelBuilder.ApplyConfiguration(new EventMasterConfig());

            // inventory
            modelBuilder.ApplyConfiguration(new OrderwiseStockConfig());
            modelBuilder.ApplyConfiguration(new OrderwiseStockMasterConfig());

            modelBuilder.ApplyConfiguration(new DocumentSequenceConfig());
            modelBuilder.ApplyConfiguration(new DepartmentConfig());
            modelBuilder.ApplyConfiguration(new OrderwiseStockTransactionConfig());

            // shipments
            modelBuilder.ApplyConfiguration(new PartShipmentConfig());
            modelBuilder.ApplyConfiguration(new QuotaTransactionConfig());

            // system configuration
            modelBuilder.ApplyConfiguration(new SystemParameterConfig());
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer("ApparelProConnection");
        //}
    }
}
