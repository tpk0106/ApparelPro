namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IPackingListService
{
    public class PackingListDetailServiceModel
    {
        public string? Detail { get; set; }
        public List<PackingListCartonDetailServiceModel> CartonRows { get; set; } = new();
        public List<PackingListStringDetailServiceModel> StringRows { get; set; } = new();
    }
}
