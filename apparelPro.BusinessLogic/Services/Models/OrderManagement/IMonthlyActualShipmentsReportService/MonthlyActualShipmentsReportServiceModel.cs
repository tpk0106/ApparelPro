namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IMonthlyActualShipmentsReportService
{
    // Replicates OD_ACTSP.PRG's "MONTHLY ACTUAL SHIPMENTS" report - every commercial
    // invoice line shipped within the given Month/Year, joined back to its invoice
    // header (for the ship date) and to Style (for the unit price used to compute
    // Value). Buyer/Type are resolved to names as enrichment - legacy's own printed
    // layout doesn't carry them at all (a single month can span many buyers/orders),
    // same convention already used to enrich other flat-table reports in this project.
    public class MonthlyActualShipmentRowServiceModel
    {
        public string InvoiceNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string OrderNo { get; set; } = null!; // new_order
        public string StyleCode { get; set; } = null!;
        public DateTime ShipDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Balance { get; set; }
        public decimal Value { get; set; } // qty * Style.UnitPrice
    }

    public class MonthlyActualShipmentsReportServiceModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<MonthlyActualShipmentRowServiceModel> Rows { get; set; } = new();
    }
}
