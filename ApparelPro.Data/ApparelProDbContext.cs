using ApparelPro.Data.Configurations.OrderManagement;
using ApparelPro.Data.Configurations.References;
using ApparelPro.Data.Configurations.Registration;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.References;
using ApparelPro.Data.Models.Registration;
using Microsoft.EntityFrameworkCore;

namespace ApparelPro.Data
{
    public class ApparelProDbContext:DbContext
    {
        public ApparelProDbContext()
        {            
        }
        public ApparelProDbContext(DbContextOptions<ApparelProDbContext> options):base(options) 
        { 
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
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<Stock> Stocks { get; set; }
        public virtual DbSet<Feature> Features { get; set; }
        public virtual DbSet<StockItem> StockItems { get; set; }       
        public virtual DbSet<PODetails>  PODetails { get; set; }
        public virtual DbSet<CurrencyExchange> CurrencyExchanges { get; set; }
        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public virtual DbSet<CurrencyConversion> CurrencyConversions { get; set; }

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
            modelBuilder.ApplyConfiguration(new UnitConfig());
            modelBuilder.ApplyConfiguration(new FeatureConfig());
            modelBuilder.ApplyConfiguration(new StockConfig());
            modelBuilder.ApplyConfiguration(new StockItemConfig());
            modelBuilder.ApplyConfiguration(new PODetailsConfig());
            modelBuilder.ApplyConfiguration(new CurrencyExchangeConfig());
            modelBuilder.ApplyConfiguration(new UserConfig());

            modelBuilder.ApplyConfiguration(new PurchaseOrderConfig());
            modelBuilder.ApplyConfiguration(new CurrencyConversionConfig());
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer("ApparelProConnection");              
        //}
    }

   
}
