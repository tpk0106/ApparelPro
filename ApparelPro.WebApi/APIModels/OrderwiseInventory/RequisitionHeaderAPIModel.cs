namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class RequisitionHeaderAPIModel
    {
        public string SrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
    }
}
