namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockMasterRowAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
        public decimal QtyInHand { get; set; }
    }
}
