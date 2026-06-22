using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class PODetailLineServiceModel
    {
        [Required] public string ItemCode { get; set; } = null!; // Keep base code for inventory reference

        // FIXED: Explicitly accept separate feature tracking fields straight from the UI!
        public string Feature1 { get; set; } = "";
        public string Feature2 { get; set; } = "";
        public string Feature3 { get; set; } = "";
        public string Feature4 { get; set; } = "";

        public string RefNo { get; set; } = "";
        [Required] public string OrderUnit { get; set; } = null!;
        [Required] public decimal OrderQuantity { get; set; }
        [Required] public decimal UnitPrice { get; set; }
        public DateTime? ExportDate { get; set; }
        public string LcNo { get; set; } = "";
    }
}
