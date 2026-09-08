using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CompanyAddressConfig : IEntityTypeConfiguration<CompanyAddress>
    {
        public void Configure(EntityTypeBuilder<CompanyAddress> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).HasColumnType("varchar(40)");
            entity.Property(e => e.Address1).HasColumnType("varchar(40)");
            entity.Property(e => e.Address2).HasColumnType("varchar(40)");
            entity.Property(e => e.City).HasColumnType("varchar(40)");
            entity.Property(e => e.PostCode).HasColumnType("varchar(15)");
            entity.Property(e => e.Country).HasColumnType("varchar(40)");
            entity.Property(e => e.TelNos).HasColumnType("varchar(40)");
            entity.Property(e => e.FaxNos).HasColumnType("varchar(40)");
            entity.Property(e => e.TinNo).HasColumnType("varchar(15)");
            entity.Property(e => e.ExportRegNo).HasColumnType("varchar(6)");
        }
    }
}
