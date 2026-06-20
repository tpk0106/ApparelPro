using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class PODetailLineAPIModel
    {
        [Required] public string ItemCode { get; set; } = null!;
        public string RefNo { get; set; } = "";
        [Required] public string OrderUnit { get; set; } = null!;
        [Required] public decimal OrderQuantity { get; set; }
        [Required] public decimal UnitPrice { get; set; }
        public DateTime? ExportDate { get; set; }
        public string LcNo { get; set; } = "";
    }
}
