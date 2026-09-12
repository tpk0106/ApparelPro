namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class PackingListDetailAPIModel
    {
        public string? Detail { get; set; }
        public List<PackingListCartonDetailAPIModel> CartonRows { get; set; } = new();
        public List<PackingListStringDetailAPIModel> StringRows { get; set; } = new();
    }
}
