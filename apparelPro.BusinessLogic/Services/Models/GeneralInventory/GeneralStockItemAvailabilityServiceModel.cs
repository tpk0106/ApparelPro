namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockItemAvailabilityServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal PhysicalQtyInHand { get; set; }
        public decimal ShadowAllocatedBalance { get; set; }
        public decimal NetAvailableBalance => PhysicalQtyInHand - ShadowAllocatedBalance;
    }
}
