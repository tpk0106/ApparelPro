namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGinIssuableStrnLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal RequestedQuantity { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal ShadowBalance { get; set; }
        public decimal MinStock { get; set; }
    }
}
