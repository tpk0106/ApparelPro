using ApparelPro.Data.Models.Registration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Registration
{
    public class ApparelProUserConfig:IEntityTypeConfiguration<ApparelProUser>
    {
        public void Configure(EntityTypeBuilder<ApparelProUser> entity)
        {
            entity.Property(c => c.DateOfBirth)
                .HasColumnType("date")
                .HasColumnName("DateOfBirth");

            entity.Property(c => c.Gender)
                    .HasColumnType("int");                    

            entity.Property(c => c.City)
                    .HasColumnType("nvarchar")
                    .HasMaxLength(50);

            entity.Property(c => c.Country)
                  .HasColumnType("nvarchar")
                  .HasMaxLength(100);

            entity.Property(c => c.AddressId)
                 .HasColumnType("int");                 

            entity.Property(c => c.KnownAs)
                    .HasColumnType("nvarchar")
                    .HasMaxLength(100);

            entity.Property(c => c.ProfilePhoto)
                    .HasColumnType("varbinary(MAX)");

            entity.Property(c => c.Created)
                  .HasColumnType("datetime")
                  .HasColumnName("CreatedDate");

            entity.Property(c => c.LastActive)
                .HasColumnType("datetime");

            entity.Property(c => c.RefreshToken)                
                .HasColumnType("nvarchar(MAX)");

            entity.Property(c => c.RefreshTokenExpiry)
                .HasColumnType("datetime")
                .HasColumnName("RefreshTokenExpiryDate");
        }
    }
}
