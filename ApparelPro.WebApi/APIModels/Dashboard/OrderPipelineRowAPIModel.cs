namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class OrderPipelineRowAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }

        public int Stage { get; set; }
        public int DaysInStage { get; set; }
        public bool IsOverdue { get; set; }

        public MerchandisingStageDetailAPIModel Merchandising { get; set; } = new();
        public ApprovalStageDetailAPIModel Approval { get; set; } = new();
        public SupplierPoStageDetailAPIModel SupplierPo { get; set; } = new();
        public GrnStageDetailAPIModel Grn { get; set; } = new();
        public ProductionStageDetailAPIModel Production { get; set; } = new();
        public ShipmentStageDetailAPIModel Shipment { get; set; } = new();
    }
}
