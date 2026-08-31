namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockStatusReportLineAPIModel
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
        public decimal TotalSrns { get; set; }
        public decimal TotalDgns { get; set; }
        public decimal? LastSan { get; set; }
        public decimal CarriedForwardBalance { get; set; }
    }
}
