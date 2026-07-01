using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderwiseInventory
{
    public class DocumentSequenceConfig:IEntityTypeConfiguration<DocumentSequence>
    {
        public void Configure(EntityTypeBuilder<DocumentSequence> entity)
        {
            entity.ToTable("DocumentSequences");
            entity.HasKey(e => e.NoteType);
            entity.Property(e => e.NoteType).HasColumnType("varchar(5)").IsRequired();
            entity.Property(e => e.Prefix).HasColumnType("varchar(3)").HasDefaultValue("");
            entity.Property(e => e.LastAllocatedNumber).IsRequired();
        }

    }
}
