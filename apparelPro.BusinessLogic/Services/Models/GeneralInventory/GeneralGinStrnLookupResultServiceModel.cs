namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGinStrnLookupResultServiceModel
    {
        public string SrnNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public string DepartmentCode { get; set; } = null!;
        public List<GeneralGinIssuableStrnLineServiceModel> Lines { get; set; } = new();
    }
}
