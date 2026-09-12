namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class PackingListStringDetailAPIModel
    {
        public int Id { get; set; }
        public int BarNo { get; set; }
        public int FromStringNo { get; set; }
        public int ToStringNo { get; set; }
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public decimal Qty { get; set; }
    }
}
