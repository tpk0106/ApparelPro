namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IYearSeasonOrdersReportService
{
    // Replicates OD_RPO2.PRG's "ORDER CONFIRMATION REPORT" family (mr_dart1-4). Legacy
    // printed four column layouts that only differ in which already-filtered Year/Season
    // columns get hidden - a dot-matrix ditto convention, same as Scheduled Shipments
    // Report's sha1/sha2/sha3. This report always returns every column and narrows rows
    // via the optional Year/Season filters instead.
    public class YearSeasonOrderStyleServiceModel
    {
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class YearSeasonOrderRowServiceModel
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
        public List<YearSeasonOrderStyleServiceModel> Styles { get; set; } = new();
        public decimal GrandTotalValue { get; set; }
    }

    public class YearSeasonOrdersReportServiceModel
    {
        public int? Year { get; set; }
        public string? Season { get; set; }
        public List<YearSeasonOrderRowServiceModel> Rows { get; set; } = new();
    }
}
