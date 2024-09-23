namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService
{
    public class CreateStyleDetailsServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; }
        public string? Unit { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? ExportBalance { get; set; }
        public bool? SupplierReturn { get; set; }
        public bool? CustomerReturn { get; set; }
        public string? Username { get; set; }
        public DateTime ApprovedDate { get; set; } = DateTime.MinValue;
        public DateTime ProductionEndDate { get; set; } = DateTime.MinValue;
        public DateTime EstimateApprovalDate { get; set; } = DateTime.MinValue;
        public string? EstimateApprovalUserName { get; set; }
        public bool? Exported { get; set; }
    }
}
