using ApparelPro.Data.Models.Registration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Registration
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(k => k.Email);
            entity.Property(p => p.Email)
                .ValueGeneratedNever()
                .HasColumnType("nvarchar")
                .HasMaxLength(100);

            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p => p.UserName)
                .HasMaxLength(30)
                .IsRequired()
                .HasColumnType("nvarchar");

            entity.Property(c => c.PasswordSalt)
                   .IsRequired(true)
                   .HasColumnType("varbinary(MAX)");

            entity.Property(c => c.PasswordHash)
                .IsRequired(true)
                .HasColumnType("varbinary(MAX)");

            entity.Property(c => c.DateOfBirth)
                .HasColumnType("date")
                .HasColumnName("DateOfBirth");

            entity.Property(c => c.Gender)
                .HasColumnType("nvarchar")
                .HasMaxLength(6);

            entity.Property(c => c.City)
                .HasColumnType("nvarchar")
                .HasMaxLength(50);

            entity.Property(c => c.Country)
              .HasColumnType("nvarchar")
              .HasMaxLength(100);

            entity.Property(c => c.KnownAs)
                .HasColumnType("nvarchar")
                .HasMaxLength(100);

            entity.Property(c => c.Photo)
                .HasColumnType("varbinary(MAX)");

            entity.Property(c => c.Created)
              .HasColumnType("datetime")
              .HasColumnName("CreatedDate");

            entity.Property(c => c.LastActive)
            .HasColumnType("datetime");
        }
    }
}
