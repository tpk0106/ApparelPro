namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    // Stage 3 - OrderwiseStockMaster.OrderedQuantity vs ReceivedQuantity is a
    // direct, already-maintained figure (GoodsReceivedNoteService increments
    // ReceivedQuantity on every GRN commit). OrderwiseStockMaster has no
    // StyleCode of its own, so attributing ordered/received amounts to one
    // style (in a multi-style order) is done by joining on ItemCode through
    // that style's own SupplierPurchaseOrderDetails rows.
    public class GrnStageDetailServiceModel
    {
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal OrderedValue { get; set; }
        public decimal ReceivedValue { get; set; }
    }
}
