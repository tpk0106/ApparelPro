using ApparelPro.Data.Models.SystemConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.SystemConfiguration
{
    public class SystemParameterConfig : IEntityTypeConfiguration<SystemParameter>
    {
        public void Configure(EntityTypeBuilder<SystemParameter> entity)
        {
            entity.ToTable("SystemParameters");
            entity.HasKey(e => e.ParameterKey);
            entity.Property(e => e.ParameterKey).HasColumnType("varchar(50)").IsRequired();
            entity.Property(e => e.Value).HasColumnType("varchar(200)").IsRequired();
            entity.Property(e => e.Description).HasColumnType("varchar(1000)");
            entity.Property(e => e.Category).HasColumnType("varchar(50)").IsRequired().HasDefaultValue("General");
            entity.Property(e => e.DataType).HasColumnType("varchar(20)").IsRequired().HasDefaultValue("Text");
            entity.Property(e => e.Options).HasColumnType("varchar(1000)");

            // Seed the one parameter this feature needs. Default OFF (strict enforcement):
            // when an apparel buyer confirms an order quantity, that figure is the contractual
            // source of truth, so style-level entries must reconcile to it unless an admin
            // explicitly opts an order into being overridden.
            entity.HasData(new SystemParameter
            {
                ParameterKey = "AllowOrderQuantityOverride",
                Value = "false",
                Description = "When false (default), the sum of a purchase order's style quantities " +
                    "(converted into the order's own unit) cannot exceed the order's Total Quantity - " +
                    "Add/Update Style Details is rejected if it would. When true, the save is allowed " +
                    "even if it exceeds Total Quantity (the Styles grid still visually flags the overage).",
                Category = "Order Management",
                DataType = "Boolean"
            });
        }
    }
}
