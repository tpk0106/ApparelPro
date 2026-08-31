namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockMasterUpdateAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;

        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
    }
}
