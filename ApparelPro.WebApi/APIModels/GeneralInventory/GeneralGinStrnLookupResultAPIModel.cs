namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGinStrnLookupResultAPIModel
    {
        public string SrnNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public string DepartmentCode { get; set; } = null!;
        public List<GeneralGinIssuableStrnLineAPIModel> Lines { get; set; } = new();
    }
}
