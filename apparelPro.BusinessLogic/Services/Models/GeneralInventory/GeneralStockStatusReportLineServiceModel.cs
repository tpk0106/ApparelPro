namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockStatusReportLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;

        public decimal BroughtForwardBalance { get; set; }
        public decimal TotalGrns { get; set; }
        public decimal TotalGins { get; set; }
        public decimal TotalGtnsIn { get; set; }
        public decimal TotalGtnsOut { get; set; }
        public decimal TotalRtns { get; set; }
        public decimal TotalSrns { get; set; } // Supplier Return Note (SRTN), both Regular+Damaged legs
        public decimal TotalDgns { get; set; }
        public decimal? LastSan { get; set; } // last Stock Adjustment Note's SET quantity this month, if any
        public decimal CarriedForwardBalance { get; set; }
    }
}
