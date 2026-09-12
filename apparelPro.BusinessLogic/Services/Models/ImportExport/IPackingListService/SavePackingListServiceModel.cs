namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IPackingListService
{
    public class SavePackingListServiceModel
    {
        public string InvoiceNumber { get; set; } = "";
        public int BuyerCode { get; set; }
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public string NewOrder { get; set; } = "";
        public string? Detail { get; set; }
        public List<PackingListCartonDetailServiceModel> CartonRows { get; set; } = new();
        public List<PackingListStringDetailServiceModel> StringRows { get; set; } = new();
    }
}
