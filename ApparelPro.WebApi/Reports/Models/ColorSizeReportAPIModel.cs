namespace ApparelPro.WebApi.Reports.Models
{
    public class ColorSizeReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";

        public List<string> SizeColumns { get; set; } = new();
        public List<ColorSizeReportStyleAPIModel> Styles { get; set; } = new();
    }

    public class ColorSizeReportStyleAPIModel
    {
        public string StyleCode { get; set; } = "";
        public List<ColorSizeReportColourAPIModel> Colours { get; set; } = new();

        public Dictionary<string, decimal> SizeTotals { get; set; } = new();
        public decimal GrandTotal { get; set; }
    }

    public class ColorSizeReportColourAPIModel
    {
        public string ColorCode { get; set; } = "";
        public string Description { get; set; } = "";

        public Dictionary<string, decimal> SizeQuantities { get; set; } = new();
        public decimal TotalQuantity { get; set; }
    }
}
