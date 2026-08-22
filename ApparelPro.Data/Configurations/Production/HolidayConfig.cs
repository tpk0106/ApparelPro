using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class HolidayConfig : IEntityTypeConfiguration<Holiday>
    {
        public void Configure(EntityTypeBuilder<Holiday> entity)
        {
            entity.ToTable("Holidays");
            entity.HasKey(e => e.Date);
            entity.Property(e => e.Date).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(50).HasColumnType("nvarchar").IsRequired();
        }
    }
}
