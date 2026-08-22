using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class ComponentOperationTemplateConfig : IEntityTypeConfiguration<ComponentOperationTemplate>
    {
        public void Configure(EntityTypeBuilder<ComponentOperationTemplate> entity)
        {
            entity.ToTable("ComponentOperationTemplates");

            entity.HasKey(e => new { e.ComponentCode, e.OperationSequence });

            entity.Property(e => e.ComponentCode).HasColumnType("nvarchar(4)").IsRequired();
            entity.Property(e => e.OperationCode).HasColumnType("nvarchar(4)").IsRequired();
            entity.Property(e => e.MachineTypeCode).HasColumnType("nvarchar(2)").IsRequired();
            entity.Property(e => e.Sam).HasColumnType("decimal(5,2)");
            entity.Property(e => e.NumberOfMachines).HasColumnType("decimal(9,2)");

            entity.HasOne<GarmentComponent>()
                .WithMany()
                .HasForeignKey(e => e.ComponentCode)
                .HasPrincipalKey(c => c.ComponentCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Operation>()
                .WithMany()
                .HasForeignKey(e => e.OperationCode)
                .HasPrincipalKey(o => o.OperationCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MachineType>()
                .WithMany()
                .HasForeignKey(e => e.MachineTypeCode)
                .HasPrincipalKey(m => m.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
