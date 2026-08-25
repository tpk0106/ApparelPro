namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGinIssuableStrnLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal RequestedQuantity { get; set; } // the SRN's original qty - GIN's default editable value
        public decimal QtyInHand { get; set; }
        public decimal ShadowBalance { get; set; }
        public decimal MinStock { get; set; }
    }
}
