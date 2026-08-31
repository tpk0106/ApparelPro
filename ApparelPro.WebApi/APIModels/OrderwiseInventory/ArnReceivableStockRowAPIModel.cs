namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ArnReceivableStockRowAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = "";
        public string AdditionalProcessCode { get; set; } = null!;
        public decimal ToDateIssued { get; set; }
        public decimal ToDateReceived { get; set; }
        public bool IsSemiFinishedGarment { get; set; }
        public decimal ReceivableBalance { get; set; }
    }
}
