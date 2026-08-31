namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockMovementReportHeaderServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public string ItemCode { get; set; } = null!;
        public string ItemDescription { get; set; } = "";
        public string Unit { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BroughtForwardBalance { get; set; }
        public decimal CarriedForwardBalance { get; set; }
        public int TransactionCount { get; set; }
    }
}
