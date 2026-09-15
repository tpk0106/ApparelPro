using ApparelPro.Data;
using ApparelPro.Data.DomainEvents;
using ApparelPro.Data.Models.Dashboard;
using ApparelPro.Data.Models.OrderManagement;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.DomainEventHandlers
{
    // Same table DashboardService.GetOrderPipelineAsync already reads
    // (reconcile-on-read) - this just writes the pipeline-stage row the
    // moment it actually happens, instead of waiting for the next dashboard
    // load to notice it. Reconcile-on-read stays in place as the fallback
    // for every other stage until they're migrated the same way.
    //
    // CORRECTED 2026-09-15: a style's Stage 2 ("Supplier PO") begins the
    // moment Approval finishes and it first NEEDS a PO - not when a PO is
    // actually raised. Raising the LAST PO line that fully covers a style's
    // material budget is what ENDS stage 2 and BEGINS stage 3 ("GRN" -
    // waiting on goods receipt). This handler previously stamped Stage 2 on
    // every PO raise, which had the direction backwards - fixed to instead
    // check whether every material line is now covered (mirrors
    // DashboardService's own `poDone` condition exactly) and, only then,
    // record the style's entry into Stage 3.
    public class AdvancePipelineOnSupplierPurchaseOrderRaised
        : IDomainEventHandler<SupplierPurchaseOrderRaisedEvent>
    {
        private const int GrnStage = 3; // matches the stage numbering DashboardService already uses

        private readonly ApparelProDbContext _apparelProDbContext;
        public AdvancePipelineOnSupplierPurchaseOrderRaised(ApparelProDbContext apparelProDbContext) => _apparelProDbContext = apparelProDbContext;

        public async Task HandleAsync(SupplierPurchaseOrderRaisedEvent e, CancellationToken ct)
        {
            // Mirrors DashboardService.GetOrderPipelineAsync's own
            // `poDone = costProfiles.Count == 0 || outstandingLines.Count == 0`
            // exactly - this PO raise only completes stage 2 if it happened
            // to be the one that cleared the LAST outstanding material line.
            var stillOutstanding = await _apparelProDbContext.StyleMaterialCostProfiles.AnyAsync(c =>
                c.BuyerCode == e.Buyer && c.Order == e.Order &&
                c.TypeCode == e.Type && c.StyleCode == e.Style &&
                c.BalanceQuantity > 0, ct);

            if (stillOutstanding) return; // still mid-stage-2 - nothing to advance yet

            var alreadyRecorded = await _apparelProDbContext.OrderPipelineStageHistories.AnyAsync(h =>
                h.BuyerCode == e.Buyer && h.Order == e.Order &&
                h.TypeCode == e.Type && h.StyleCode == e.Style &&
                h.Stage == GrnStage, ct);

            if (alreadyRecorded) return; // idempotent - safe if the handler ever runs twice

            _apparelProDbContext.OrderPipelineStageHistories.Add(new OrderPipelineStageHistory
            {
                BuyerCode = e.Buyer,
                Order = e.Order,
                TypeCode = e.Type,
                StyleCode = e.Style,
                Stage = GrnStage,
                EnteredAt = DateTime.UtcNow,
            });
            await _apparelProDbContext.SaveChangesAsync(ct);
        }
    }
}
