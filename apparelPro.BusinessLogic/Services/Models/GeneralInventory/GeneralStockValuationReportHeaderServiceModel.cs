namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockValuationReportHeaderServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public string FromItemCode { get; set; } = null!;
        public string ToItemCode { get; set; } = null!;
        public decimal TotalValue { get; set; }
        public int TotalLineItems { get; set; }
    }
}
