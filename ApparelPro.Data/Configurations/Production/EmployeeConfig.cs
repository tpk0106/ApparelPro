using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class EmployeeConfig : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> entity)
        {
            entity.ToTable("Employees");
            entity.HasKey(x => x.EmployeeCode);
            entity.Property(p => p.EmployeeCode)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(4)
                .HasColumnType("nvarchar");
            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");
        }
    }
}
