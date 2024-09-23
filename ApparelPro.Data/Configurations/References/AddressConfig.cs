using ApparelPro.Data.Models.References;
using ApparelPro.Data.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class AddressConfig : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> entity)
        {
            entity.HasKey(k => new { k.Id, k.AddressId});

            entity.Property(p => p.Id)
                .IsRequired()
                .HasColumnType("int")
                .UseIdentityColumn();

            // https://stackoverflow.com/questions/60909477/ef-core-3-has-value-generator
            entity.Property(p => p.AddressId)
                .IsRequired(true)
                .HasColumnType("Guid");
                //.ValueGeneratedOnAdd()
                //.HasValueGenerator<ApparelProAddressIdValueGenerator>();
             //  .HasDefaultValue(new Guid())



            //entity.HasOne<Buyer>(p => p.AddressId).WithMany<Address>(p => p.AddressId);
            //entity.HasOne<Buyer>().WithMany<Address>().HasForeignKey(p=>p.AddressId);

            ////entity.Property(p => p.AddressType)
            ////    .HasColumnType(typeof(Constants.AddressType).ToString());

            //var converter = new ValueConverter<AddressType, string>(
            //    v => v.ToString(),
            //    v => (AddressType)Enum.Parse(typeof(AddressType), v));

            //entity.Property(p => p.AddressType)
            //    .HasConversion(converter);
            ////   // .HasConversion(val => val, v => (Constants.AddressType)Convert.ChangeType(v, typeof(Constants.AddressType)));

            //entity.Property(p => p.AddressType)
            //    .HasColumnType("int")
            //    .IsRequired(false);

            entity.Property(p => p.AddressType)
                .HasConversion<int>()
               .IsRequired(false);

            entity.Property(p => p.Default)
                .HasColumnType("bit")
                .IsRequired(false);
              //  .HasDefaultValue(false);                       

            entity.Property(p => p.StreetAddress)
             .HasMaxLength(200)
             .IsRequired(false)
             .HasColumnType("nvarchar");

            entity.Property(p => p.City)
             .HasMaxLength(100)
             .IsRequired(false)
             .HasColumnType("nvarchar");
            

            entity.Property(p => p.CountryCode)
             .HasMaxLength(2)
             .IsRequired(false)
             .HasColumnType("nvarchar");

            entity.Property(p => p.State)
            .HasMaxLength(3)
            .IsRequired(false)
            .HasColumnType("nvarchar");

            entity.Property(p => p.PostCode)
            .HasMaxLength(5)
            .IsRequired(false)
            .HasColumnType("nvarchar");
        }
    }
}
