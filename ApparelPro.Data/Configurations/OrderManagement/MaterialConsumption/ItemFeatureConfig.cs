using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption
{
    public class ItemFeatureConfig : IEntityTypeConfiguration<ItemFeature>
    {
        public void Configure(EntityTypeBuilder<ItemFeature> entity)
        {
            entity.ToTable("ItemFeatures");
            entity.HasKey(e => e.FeatureCode);

            // Aligned to your clean, corporate naming style
            entity.Property(e => e.FeatureCode).HasColumnType("varchar(4)").HasColumnName("FeatureCode");
            entity.Property(e => e.Description).HasColumnType("varchar(30)").HasColumnName("Description");

        }
    }
}
