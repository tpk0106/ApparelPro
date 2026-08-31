namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockReorderReportHeaderServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public int TotalLineItems { get; set; }
    }
}
