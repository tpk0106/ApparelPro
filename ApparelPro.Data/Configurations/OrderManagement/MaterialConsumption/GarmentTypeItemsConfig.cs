using ApparelPro.WebApi.APIModels.OrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption
{
    public class GarmentTypeItemsConfig:IEntityTypeConfiguration<GarmentTypeItems>
    {
        public void Configure(EntityTypeBuilder<GarmentTypeItems> entity)
        {
            entity.ToTable("GarmentTypeItems"); // Links explicitly to your chosen table name

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn(); // Auto-incrementing primary key ID

            entity.Property(e => e.GarmentTypeId).HasColumnName("GarmentTypeId").IsRequired();
            entity.Property(e => e.StockCode).HasColumnType("varchar(2)").HasColumnName("StockCode").IsRequired();
            entity.Property(e => e.ItemCode).HasColumnType("varchar(6)").HasColumnName("ItemCode").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit").IsRequired();
            entity.Property(e => e.Quantity).HasColumnType("decimal(8,3)").HasColumnName("Quantity").IsRequired();

            // Setup the Foreign Key constraint pointing to your independent GarmentType reference file table
            entity.HasOne(e => e.GarmentType)
                  .WithMany() // Assuming a GarmentType can have many default item template rows
                  .HasForeignKey(e => e.GarmentTypeId)
                  .OnDelete(DeleteBehavior.Restrict); // Blocks accidental deletion of a core garment type reference
        }
    }
}
