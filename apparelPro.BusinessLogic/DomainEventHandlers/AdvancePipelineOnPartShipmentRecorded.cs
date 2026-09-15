using ApparelPro.Data;
using ApparelPro.Data.DomainEvents;
using ApparelPro.Data.Models.Dashboard;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.DomainEventHandlers
{
    // Same table DashboardService.GetOrderPipelineAsync already reads
    // (reconcile-on-read) - see AdvancePipelineOnSupplierPurchaseOrderRaised
    // for the full explanation of why this mirrors DashboardService's own
    // completion condition exactly instead of assuming "this save means the
    // stage is done."
    //
    // A style's Stage 5 ("Shipment") ends and Stage 6 ("Complete") begins the
    // moment its total scheduled shipment quantity meets the style's full
    // order quantity - the last stage in the pipeline.
    public class AdvancePipelineOnPartShipmentRecorded : IDomainEventHandler<PartShipmentRecordedEvent>
    {
        private const int CompleteStage = 6; // matches the stage numbering DashboardService already uses

        private readonly ApparelProDbContext _apparelProDbContext;
        public AdvancePipelineOnPartShipmentRecorded(ApparelProDbContext apparelProDbContext) => _apparelProDbContext = apparelProDbContext;

        public async Task HandleAsync(PartShipmentRecordedEvent e, CancellationToken ct)
        {
            // Mirrors DashboardService.GetOrderPipelineAsync's own
            // `shipmentTarget = style.Quantity` and
            // `shipmentDone = shipmentTarget <= 0 || scheduledShipment >= shipmentTarget`
            // exactly.
            var shipmentTarget = await _apparelProDbContext.Styles
                .Where(s => s.BuyerCode == e.Buyer && s.Order == e.Order && s.TypeCode == e.Type && s.StyleCode == e.Style)
                .Select(s => s.Quantity)
                .FirstOrDefaultAsync(ct) ?? 0;

            if (shipmentTarget <= 0) return; // nothing to compare against - not our concern

            var scheduledShipment = await _apparelProDbContext.PartShipments
                .Where(p => p.BuyerCode == e.Buyer && p.Order == e.Order && p.TypeCode == e.Type && p.StyleCode == e.Style)
                .SumAsync(p => (decimal?)p.Quantity, ct) ?? 0;

            if (scheduledShipment < shipmentTarget) return; // still mid-stage-5 - nothing to advance yet

            var alreadyRecorded = await _apparelProDbContext.OrderPipelineStageHistories.AnyAsync(h =>
                h.BuyerCode == e.Buyer && h.Order == e.Order &&
                h.TypeCode == e.Type && h.StyleCode == e.Style &&
                h.Stage == CompleteStage, ct);

            if (alreadyRecorded) return; // idempotent - safe if the handler ever runs twice

            _apparelProDbContext.OrderPipelineStageHistories.Add(new OrderPipelineStageHistory
            {
                BuyerCode = e.Buyer,
                Order = e.Order,
                TypeCode = e.Type,
                StyleCode = e.Style,
                Stage = CompleteStage,
                EnteredAt = DateTime.UtcNow,
            });
            await _apparelProDbContext.SaveChangesAsync(ct);
        }
    }
}
