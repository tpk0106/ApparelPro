using ApparelPro.Data;
using ApparelPro.Data.DomainEvents;
using ApparelPro.Data.Models.Dashboard;
using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.DomainEventHandlers
{
    // Same table DashboardService.GetOrderPipelineAsync already reads
    // (reconcile-on-read) - see AdvancePipelineOnSupplierPurchaseOrderRaised
    // for the full explanation of why this mirrors DashboardService's own
    // completion condition exactly instead of assuming "this save means the
    // stage is done."
    //
    // A style's Stage 4 ("Production") ends and Stage 5 ("Shipment") begins
    // the moment its actual produced quantity meets its target.
    public class AdvancePipelineOnProductionRecorded : IDomainEventHandler<ProductionRecordedEvent>
    {
        private const int ShipmentStage = 5; // matches the stage numbering DashboardService already uses

        private readonly ApparelProDbContext _apparelProDbContext;
        public AdvancePipelineOnProductionRecorded(ApparelProDbContext apparelProDbContext) => _apparelProDbContext = apparelProDbContext;

        public async Task HandleAsync(ProductionRecordedEvent e, CancellationToken ct)
        {
            // Mirrors DashboardService.GetOrderPipelineAsync's own
            // `productionTarget = lineAllocTarget > 0 ? lineAllocTarget : style.Quantity`
            // and `productionDone = productionTarget <= 0 || actualProduction >= productionTarget`
            // exactly.
            var lineAllocTarget = await _apparelProDbContext.ProductionLineAllocations
                .Where(a => a.BuyerCode == e.Buyer && a.Order == e.Order && a.TypeCode == e.Type && a.StyleCode == e.Style)
                .SumAsync(a => (decimal?)a.TotalQuantity, ct) ?? 0;

            decimal productionTarget = lineAllocTarget;
            if (productionTarget <= 0)
            {
                var style = await _apparelProDbContext.Styles
                    .Where(s => s.BuyerCode == e.Buyer && s.Order == e.Order && s.TypeCode == e.Type && s.StyleCode == e.Style)
                    .Select(s => s.Quantity)
                    .FirstOrDefaultAsync(ct);
                productionTarget = style ?? 0;
            }

            if (productionTarget <= 0) return; // nothing to compare against - not our concern

            var actualProduction = await _apparelProDbContext.DailyProductionEntries
                .Where(d => d.BuyerCode == e.Buyer && d.Order == e.Order && d.TypeCode == e.Type && d.StyleCode == e.Style)
                .SumAsync(d => (decimal?)d.Quantity, ct) ?? 0;

            if (actualProduction < productionTarget) return; // still mid-stage-4 - nothing to advance yet

            var alreadyRecorded = await _apparelProDbContext.OrderPipelineStageHistories.AnyAsync(h =>
                h.BuyerCode == e.Buyer && h.Order == e.Order &&
                h.TypeCode == e.Type && h.StyleCode == e.Style &&
                h.Stage == ShipmentStage, ct);

            if (alreadyRecorded) return; // idempotent - safe if the handler ever runs twice

            _apparelProDbContext.OrderPipelineStageHistories.Add(new OrderPipelineStageHistory
            {
                BuyerCode = e.Buyer,
                Order = e.Order,
                TypeCode = e.Type,
                StyleCode = e.Style,
                Stage = ShipmentStage,
                EnteredAt = DateTime.UtcNow,
            });
            await _apparelProDbContext.SaveChangesAsync(ct);
        }
    }
}
