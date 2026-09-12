namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class SavePackingListAPIModel
    {
        public string InvoiceNumber { get; set; } = "";
        public int BuyerCode { get; set; }
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public string NewOrder { get; set; } = "";
        public string? Detail { get; set; }
        public List<PackingListCartonDetailAPIModel> CartonRows { get; set; } = new();
        public List<PackingListStringDetailAPIModel> StringRows { get; set; } = new();
    }
}
