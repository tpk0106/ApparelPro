using apparelPro.BusinessLogic.Services.Models.OrderManagement.Stylewise_Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderManagement.Stylewise_events
{
    public class StylewiseEventConfig:IEntityTypeConfiguration<StylewiseEvent>
    {
        public void Configure(EntityTypeBuilder<StylewiseEvent> entity)
        {
            entity.ToTable("StylewiseEvents");
            entity.HasKey(e => e.EventCode); // Strict String Primary Key 

            entity.Property(e => e.EventCode).HasColumnType("varchar(10)").HasColumnName("EventCode").IsRequired();
            entity.Property(e => e.Description).HasColumnType("varchar(50)").HasColumnName("Description").IsRequired();
        }
    }
}
