namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GinStrnLookupResultServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public List<GinIssuableStrnLineServiceModel> Lines { get; set; } = new();
    }
}
