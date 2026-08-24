namespace ApparelPro.WebApi.Reports.Models
{
    public class YearSeasonOrderStyleAPIModel
    {
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class YearSeasonOrderRowAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string? Description { get; set; }
        public string CountryCode { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal TotalQuantity { get; set; }
        public string CurrencyCode { get; set; } = "";
        public string SeasonCode { get; set; } = "";
        public string SeasonDescription { get; set; } = "";
        public DateOnly OrderDate { get; set; }
        public List<YearSeasonOrderStyleAPIModel> Styles { get; set; } = new();
        public decimal GrandTotalValue { get; set; }
    }

    public class YearSeasonOrdersReportAPIModel
    {
        public int? Year { get; set; }
        public string? Season { get; set; }
        public List<YearSeasonOrderRowAPIModel> Rows { get; set; } = new();
    }
}
