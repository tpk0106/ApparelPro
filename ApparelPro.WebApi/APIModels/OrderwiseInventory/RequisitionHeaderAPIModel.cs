namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class RequisitionHeaderAPIModel
    {
        public string StrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
    }
}
