using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class DailyProductionTimeTicketEntryConfig : IEntityTypeConfiguration<DailyProductionTimeTicketEntry>
    {
        public void Configure(EntityTypeBuilder<DailyProductionTimeTicketEntry> entity)
        {
            entity.ToTable("DailyProductionTimeTicketEntries");

            entity.HasKey(e => new
            {
                e.Date,
                e.LineCode,
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.EmployeeCode,
                e.OperationCode
            });

            entity.Property(e => e.LineCode).HasColumnType("nvarchar(3)");
            entity.Property(e => e.Order).HasColumnType("varchar(20)");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)");
            entity.Property(e => e.EmployeeCode).HasColumnType("nvarchar(4)");
            entity.Property(e => e.OperationCode).HasColumnType("nvarchar(4)");
            entity.Property(e => e.NonProductiveHourCode).HasColumnType("nvarchar(2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(9,2)");
            entity.Property(e => e.NonProductiveHours).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WorkHours).HasColumnType("decimal(5,2)");

            entity.HasOne<ProductionLine>()
                .WithMany()
                .HasForeignKey(e => e.LineCode)
                .HasPrincipalKey(l => l.LineCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(e => e.EmployeeCode)
                .HasPrincipalKey(emp => emp.EmployeeCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Operation>()
                .WithMany()
                .HasForeignKey(e => e.OperationCode)
                .HasPrincipalKey(o => o.OperationCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<NonProductiveHourCode>()
                .WithMany()
                .HasForeignKey(e => e.NonProductiveHourCode)
                .HasPrincipalKey(n => n.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
