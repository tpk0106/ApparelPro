
using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    public class Style
    {
        [NotMapped]
        public int Id { get; set; }
        public int BuyerCode { get; set; }
        [NotMapped]
        public string Buyer { get; set; }
        public string Order { get; set; }

        // Keeps track of when the order was placed (non-nullable if always known)
        public DateOnly OrderDate { get; set; }


        public int TypeCode { get; set; }
        [NotMapped]
        public string? Type { get; set; }
        public string StyleCode { get; set; }
        public string? Unit { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ColorRatio {  get; set; } = null;
        public string? SizeRatio { get; set; } = null; 
        public decimal? ExportBalance { get; set; }
        public bool? SupplierReturn { get; set; }
        public bool? CustomerReturn { get; set; }
        public string? Username { get; set; }

        // Made these nullable DateOnly because they are filled out later
        public DateOnly? ApprovedDate { get; set; }
        public DateOnly? ProductionEndDate { get; set; }
        public DateOnly? EstimateApprovalDate { get; set; }


        public string? EstimateApprovalUserName { get; set; }
        public bool? Exported { get; set; }
    }
}
