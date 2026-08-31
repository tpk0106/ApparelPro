namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockMasterEntryAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string? Feature1 { get; set; }
        public string? Feature2 { get; set; }
        public string? Feature3 { get; set; }
        public string? Feature4 { get; set; }

        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
    }
}
