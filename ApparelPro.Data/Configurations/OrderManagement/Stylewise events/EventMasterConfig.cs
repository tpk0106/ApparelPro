using apparelPro.BusinessLogic.Services.Models.OrderManagement.Stylewise_Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderManagement.Stylewise_events
{
    public class EventMasterConfig:IEntityTypeConfiguration<EventMaster>
    {
        public void Configure(EntityTypeBuilder<EventMaster> entity)
        {
            entity.ToTable("EventMasters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            // Establish unique composite constraint index tracking to prevent duplicate milestone injections under one style scope
            entity.HasIndex(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.EventCode }).IsUnique();

            entity.Property(e => e.BuyerCode).HasColumnName("BuyerCode").IsRequired();
            entity.Property(e => e.Order).HasColumnType("varchar(12)").HasColumnName("Order").IsRequired();
            entity.Property(e => e.TypeCode).HasColumnName("TypeCode").IsRequired();
            entity.Property(e => e.StyleCode).HasColumnType("varchar(12)").HasColumnName("StyleCode").IsRequired();
            entity.Property(e => e.EventCode).HasColumnType("varchar(10)").HasColumnName("EventCode").IsRequired();

            entity.Property(e => e.ScheduledDate).HasColumnType("date").HasColumnName("ScheduledDate");
            entity.Property(e => e.ActualDate).HasColumnType("date").HasColumnName("ActualDate");
            entity.Property(e => e.Remarks).HasColumnType("varchar(100)").HasColumnName("Remarks");

            // Enforce safe delete behavior constraints 
            entity.HasOne(e => e.MilestoneEvent)
                  .WithMany()
                  .HasForeignKey(e => e.EventCode)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
