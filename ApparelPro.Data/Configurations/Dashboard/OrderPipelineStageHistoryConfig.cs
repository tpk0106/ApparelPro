using ApparelPro.Data.Models.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Dashboard
{
    public class OrderPipelineStageHistoryConfig : IEntityTypeConfiguration<OrderPipelineStageHistory>
    {
        public void Configure(EntityTypeBuilder<OrderPipelineStageHistory> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Order)
                .IsRequired()
                .HasColumnType("nvarchar(12)");

            entity.Property(e => e.StyleCode)
                .IsRequired()
                .HasColumnType("nvarchar(12)");

            entity.Property(e => e.EnteredAt)
                .IsRequired();

            // One row per style per stage it has ever reached - the
            // reconcile-on-read logic in OrderPipelineService checks this
            // index before deciding whether a new row is needed.
            entity.HasIndex(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.Stage })
                .IsUnique();
        }
    }
}
