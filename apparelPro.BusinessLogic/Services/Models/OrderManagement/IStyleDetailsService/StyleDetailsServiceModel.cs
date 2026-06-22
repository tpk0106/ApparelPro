using System.ComponentModel.DataAnnotations.Schema;

namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService
{
    public class StyleDetailsServiceModel
    {        
        //public int Id { get; set; }
        public int BuyerCode { get; set; }        
        public string Buyer { get; set; }
        public string Order { get; set; }        
        public DateOnly OrderDate { get; set; }
        public int TypeCode { get; set; }        
        //public string? Type { get; set; }
        public string StyleCode { get; set; }
        public string? Unit { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ColorRatio { get; set; } = null;
        public string? SizeRatio { get; set; } = null;

        //public decimal? ExportBalance { get; set; }
        //public bool? SupplierReturn { get; set; }
        //public bool? CustomerReturn { get; set; }
        //public string? Username { get; set; }        
        //public DateOnly? ApprovedDate { get; set; }
        //public DateOnly? ProductionEndDate { get; set; }
        //public DateOnly? EstimateApprovalDate { get; set; }
        //public string? EstimateApprovalUserName { get; set; }
        //public bool? Exported { get; set; }
    }
}
