namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    // Stage 4 - v1 deliberately reports one overall progress figure rather
    // than a per-section (Cutting/Sewing/Finishing/Packing) breakdown, per
    // product decision 2026-09-06: DailyProductionEntry.SectionCode exists,
    // but there's no confirmed per-section target (StyleOperationBreakdown
    // is per-operation, not per-section, so operation->section mapping
    // would need its own follow-up work). ActualQuantity is
    // SUM(DailyProductionEntry.Quantity) for this style (no cumulative total
    // is stored anywhere - computed fresh on every read, matching how this
    // table is read elsewhere). TargetQuantity prefers the sum of this
    // style's ProductionLineAllocation.TotalQuantity rows (the actual
    // planned/allocated total), falling back to the style's own order
    // Quantity when no line allocation exists yet.
    public class ProductionStageDetailServiceModel
    {
        public decimal TargetQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
    }
}
