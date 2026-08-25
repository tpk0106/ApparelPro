namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGrnReceivableLineAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public decimal Balance { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal MaxStock { get; set; }
    }
}
