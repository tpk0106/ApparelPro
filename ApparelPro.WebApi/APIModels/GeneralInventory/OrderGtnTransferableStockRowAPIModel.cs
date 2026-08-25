namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class OrderGtnTransferableStockRowAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal AvailableBalance { get; set; }
    }
}
