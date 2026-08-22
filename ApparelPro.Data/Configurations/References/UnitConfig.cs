using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class UnitConfig : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> entity)
        {
            entity.HasKey(k => k.Id);
            // Fix (2026-08-16): was nvarchar, which mismatched the varchar(3) type used by
            // every table that references this column via FK - see AddTier1ReferenceDataForeignKeys
            // migration for the full narrowing steps.
            entity.Property(p => p.Code)
               .HasMaxLength(3)
               .IsRequired()
               .HasColumnType("varchar");

            entity.Property(p => p.Description)
               .HasMaxLength(30)
               .IsRequired()
               .HasColumnType("nvarchar");

            // Tier 1 relationships audit (2026-08-16): Unit's real PK is the surrogate Id, but
            // every other table references it by Code. 12 different FK declarations across the
            // codebase already call .HasPrincipalKey(u => u.Code), which requires Code to be a
            // genuine EF key - EF has been implicitly treating it as an alternate key all along
            // (that's why the live constraint is already named AK_Units_Code, EF's own default
            // alternate-key naming convention). Made explicit here rather than relying on that
            // being implied elsewhere. NOTE: this must be HasAlternateKey, not HasIndex().IsUnique() -
            // a plain unique index is a different object to SQL Server than the UNIQUE CONSTRAINT
            // that already physically exists, and mixing the two caused an invalid "DROP INDEX on a
            // constraint-backed index" error during migration.
            entity.HasAlternateKey(p => p.Code).HasName("AK_Units_Code");
        }
    }
}
