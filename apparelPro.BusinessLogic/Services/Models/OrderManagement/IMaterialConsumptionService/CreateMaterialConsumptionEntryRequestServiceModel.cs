using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class CreateMaterialConsumptionEntryRequestServiceModel
    {
        [Required] public int BuyerCode { get; set; }
        [Required] public string Order { get; set; } = null!;
        [Required] public int TypeCode { get; set; }
        [Required] public string StyleCode { get; set; } = null!;

        // Dynamic intersection constraints (Can accept empty strings for universal style rows)
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";

        [Required] public string StockCode { get; set; } = null!;
        [Required] public string ItemCode { get; set; } = null!;

        // Feature properties mapping your dynamic interface inputs
        public string Feature1 { get; set; } = "";
        public string Feature2 { get; set; } = "";
        public string Feature3 { get; set; } = "";
        public string Feature4 { get; set; } = "";

        [Required] public string ConsumptionUnit { get; set; } = null!;
        [Required] public decimal QuantityPerGarment { get; set; }
        [Required] public decimal PercentageAllowance { get; set; }
        [Required] public string ItemUnit { get; set; } = null!;
        [Required] public decimal TotalConsumption { get; set; }
        [Required] public string SupplierCode { get; set; } = null!;
        [Required] public decimal UnitPrice { get; set; }
        [Required] public string Currency { get; set; }
    }
}
