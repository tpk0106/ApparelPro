namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GinStrnLookupResultAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public List<GinIssuableStrnLineAPIModel> Lines { get; set; } = new();
    }
}
