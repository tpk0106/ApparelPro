namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class PackingListCartonDetailAPIModel
    {
        public int Id { get; set; }
        public int FromCartonNo { get; set; }
        public int ToCartonNo { get; set; }
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public decimal Qty { get; set; }
        public int NoOfCartons { get; set; }
    }
}
