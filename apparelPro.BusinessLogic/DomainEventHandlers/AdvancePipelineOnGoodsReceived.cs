using ApparelPro.Data;
using ApparelPro.Data.DomainEvents;
using ApparelPro.Data.Models.Dashboard;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.DomainEventHandlers
{
    // Same table DashboardService.GetOrderPipelineAsync already reads
    // (reconcile-on-read) - see AdvancePipelineOnSupplierPurchaseOrderRaised
    // for the full explanation of why this mirrors DashboardService's own
    // completion condition exactly instead of assuming "this GRN post means
    // the stage is done."
    //
    // A style's Stage 3 ("GRN") begins the moment its LAST required Supplier
    // PO is raised (see AdvancePipelineOnSupplierPurchaseOrderRaised, which
    // stamps that). This handler is the other end of that stage: receiving
    // the LAST outstanding quantity across every PO line item for the style
    // is what ENDS stage 3 and BEGINS stage 4 (Production).
    public class AdvancePipelineOnGoodsReceived : IDomainEventHandler<GoodsReceivedEvent>
    {
        private const int ProductionStage = 4; // matches the stage numbering DashboardService already uses

        private readonly ApparelProDbContext _apparelProDbContext;
        public AdvancePipelineOnGoodsReceived(ApparelProDbContext apparelProDbContext) => _apparelProDbContext = apparelProDbContext;

        public async Task HandleAsync(GoodsReceivedEvent e, CancellationToken ct)
        {
            // Mirrors DashboardService.GetOrderPipelineAsync's own
            // `grnDone = orderedQty <= 0 || receivedQty >= orderedQty` exactly:
            // sum Ordered/Received across every OrderwiseStockMaster row for
            // each of this style's own Supplier PO line items.
            var itemCodes = await _apparelProDbContext.SupplierPurchaseOrderDetails
                .Where(d => d.Buyer == e.Buyer && d.Order == e.Order && d.Type == e.Type && d.Style == e.Style)
                .Select(d => d.ItemCode)
                .Distinct()
                .ToListAsync(ct);

            if (itemCodes.Count == 0) return; // nothing was ever ordered for this style - not our concern

            var masters = await _apparelProDbContext.OrderwiseStockMasters
                .Where(m => m.BuyerCode == e.Buyer && m.Order == e.Order && itemCodes.Contains(m.ItemCode))
                .ToListAsync(ct);

            var orderedQty = masters.Sum(m => m.OrderedQuantity);
            var receivedQty = masters.Sum(m => m.ReceivedQuantity);
            var grnDone = orderedQty <= 0 || receivedQty >= orderedQty;

            if (!grnDone) return; // still mid-stage-3 - nothing to advance yet

            var alreadyRecorded = await _apparelProDbContext.OrderPipelineStageHistories.AnyAsync(h =>
                h.BuyerCode == e.Buyer && h.Order == e.Order &&
                h.TypeCode == e.Type && h.StyleCode == e.Style &&
                h.Stage == ProductionStage, ct);

            if (alreadyRecorded) return; // idempotent - safe if the handler ever runs twice

            _apparelProDbContext.OrderPipelineStageHistories.Add(new OrderPipelineStageHistory
            {
                BuyerCode = e.Buyer,
                Order = e.Order,
                TypeCode = e.Type,
                StyleCode = e.Style,
                Stage = ProductionStage,
                EnteredAt = DateTime.UtcNow,
            });
            await _apparelProDbContext.SaveChangesAsync(ct);
        }
    }
}
