namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GinPrintHeaderAPIModel
    {
        public string GinNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public string SourceStrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
