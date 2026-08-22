namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    public class OrderManagementSummaryServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public decimal? OrderQuantity { get; set; }
        public string? Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public DateOnly OrderDate { get; set; }
        public DateOnly? EstimateApprovalDate { get; set; }

        // Summed across every PartShipments row for this style, regardless of
        // shipment unit - a straight sum, not unit-converted, since this is a
        // dashboard-level summary rather than a financial reconciliation.
        public decimal ShippedQuantity { get; set; }

        public List<ColorSizeMixServiceModel> ColorSizeMix { get; set; } = new();
    }
}
