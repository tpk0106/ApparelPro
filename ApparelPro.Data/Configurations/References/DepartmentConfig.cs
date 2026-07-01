using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class DepartmentConfig:IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> entity)
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.DepartmentCode);
            entity.Property(e => e.DepartmentCode).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.Name).HasColumnType("varchar(30)").IsRequired();
        }
    }
}
