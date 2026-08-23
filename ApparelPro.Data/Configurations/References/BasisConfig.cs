using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class BasisConfig : IEntityTypeConfiguration<Basis>
    {
        public void Configure(EntityTypeBuilder<Basis> entity)
        {
            entity.HasKey(k => k.Id);
            entity.Property(p => p.Code)
               .HasMaxLength(3)
               .IsRequired()
               .HasColumnType("nvarchar");

            entity.Property(p => p.Description)
               .HasMaxLength(30)
               // .IsRequired()
               .HasColumnType("nvarchar");

            entity.Property(p => p.ValueAdd)
                .HasDefaultValue(false);
               //.HasColumnType("bit");

            // Tier 1 relationships audit (2026-08-16): Basis's real PK is the surrogate Id, but
            // other tables reference it by Code. Code needs its own unique constraint before
            // anything can declare a real FOREIGN KEY against it.
            entity.HasIndex(p => p.Code).IsUnique();

            // Tier 2 relationships audit: seeds the "CMP" (Cut-Make-Pack) basis row that 3
            // existing PurchaseOrder rows already reference but which was never added here -
            // required before PurchaseOrder.BasisCode -> Basis.Code can be enforced as a real FK.
            entity.HasData(
                new Basis { Id = 9, Code = "CMP", Description = "CUT, MAKE & PACK", ValueAdd = false }
            );
        }
    }
}
