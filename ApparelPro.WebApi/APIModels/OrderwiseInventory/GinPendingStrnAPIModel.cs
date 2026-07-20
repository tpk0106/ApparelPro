namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GinPendingStrnAPIModel
    {
        public string StrnNumber { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
    }
}
