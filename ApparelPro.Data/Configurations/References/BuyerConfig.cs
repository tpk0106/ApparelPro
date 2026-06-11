using ApparelPro.Data.Models.References;
//using ApparelPro.Data.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class BuyerConfig : IEntityTypeConfiguration<Buyer>
    {
        public void Configure(EntityTypeBuilder<Buyer> entity)
        {
            entity.HasKey(k => k.BuyerCode);

            entity.Property(k => k.BuyerCode)
                .IsRequired()
                .HasColumnType("int")
                .UseIdentityColumn();

            // 🚀 Configure the One-to-Many Relationship
            entity.HasMany(b => b.Addresses)      // Buyer has Many Addresses
                  .WithOne()                       // Address points back to one implicit parent
                  .HasForeignKey(a => a.BuyerCode)  // Enforces BuyerCode as the lookup key on the Address table
                  .HasPrincipalKey(b => b.BuyerCode)
                  .OnDelete(DeleteBehavior.Cascade); // If a buyer is deleted, automatically delete its 3 addresses!


            //entity.Property(p => p.AddressId)
            //     .IsRequired(false)
            //     .HasColumnType("uniqueidentifier");

            // .ValueGeneratedOnAdd()
            // .HasValueGenerator<ApparelProAddressIdValueGenerator>()

            //   entity.Property(p=>p.AddressId).UseIdentityColumn();
            //.HasColumnType("char(36)")
            //.HasColumnType("GUID")
            //  .HasDefaultValue(new Guid("00000000-0000-0000-0000-000000000000"));


            // if we dont add Buyer property in Address we can do like below
            // entity.HasOne<Address>().WithMany().HasForeignKey(p => p.AddressId).HasPrincipalKey(p => p.AddressId);

            // if we add Buyer property in Address we can do like below

            //entity.HasMany(p=>p.Addresses).WithOne(p=>p.Buyer).HasForeignKey(p => p.AddressId).HasPrincipalKey(p=>p.AddressId);

            // below is commented temprarily

            //entity.HasMany(p => p.Addresses).WithOne(p => p.Buyer).HasPrincipalKey(p => p.AddressId);

            // this changes done by thusith on 23 sep 2024 in order to remove buyer navigation property on the otherside (address)
            // no meaning to have buyer at address side. 
            //entity
            //    .HasMany(p => p.Addresses)
            //    .WithOne()
            //    .HasForeignKey(b => b.AddressId)
            //    .HasPrincipalKey(k => k.AddressId)
            //    .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Status)
               .HasMaxLength(2)
               .IsRequired()
               .HasColumnType("nvarchar");            

            entity.Property(p => p.Name)
               .HasMaxLength(200)
               .IsRequired()
               .HasColumnType("nvarchar");            

            entity.Property(p => p.TelephoneNos)
               .HasMaxLength(100)
               .IsRequired(false)
               .HasColumnType("nvarchar");

            entity.Property(p => p.MobileNos)
             .HasMaxLength(100)
             .IsRequired(false)
             .HasColumnType("nvarchar");

            entity.Property(p=>p.Fax)
             .HasMaxLength(100)
             .IsRequired(false)
             .HasColumnType("nvarchar");

            entity.Property(p => p.CUSDEC)
             .HasMaxLength(11)
             .IsRequired(false)
             .HasColumnType("nvarchar");

            entity.HasIndex(p => p.BuyerCode);
        }
    }
}
