using System;

namespace ApparelPro.Data.Models.Dashboard
{
    // One row per (BuyerCode, Order, TypeCode, StyleCode, Stage) the style has
    // ever reached - stamps the first moment OrderPipelineService computed
    // that stage for it. Purely additive/audit: nothing else in the app reads
    // or writes this table, and no existing report/screen depends on it.
    // Backs "days in current stage" / overdue on the Order pipeline dashboard
    // panel - there was no other timestamp anywhere in the schema this could
    // have been derived from (see project_order_pipeline_dashboard memory).
    public class OrderPipelineStageHistory
    {
        public int Id { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public int Stage { get; set; }
        public DateTime EnteredAt { get; set; }
    }
}
