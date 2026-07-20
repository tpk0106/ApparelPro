namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockItemAvailabilityAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal PhysicalQtyInHand { get; set; }
        public decimal ShadowAllocatedBalance { get; set; }
        public decimal RequisitionedStrnBalance { get; set; }
        public decimal NetAvailableBalance { get; set; }
    }
}
