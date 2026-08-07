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

        // Populated only when the merchandiser changed Color/Size while editing an EXISTING
        // ledger line. When set and different from Color/Size above, the old ledger row under
        // the ORIGINAL Color/Size is deleted (unless a supplier PO already draws against it) so a
        // reselect never leaves a duplicate/orphaned row behind. Leave null for a normal add.
        public string? OriginalColor { get; set; }
        public string? OriginalSize { get; set; }

        [Required] public string StockCode { get; set; } = null!;
        [Required] public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;

        // Feature properties mapping your dynamic interface inputs
        public string Feature1 { get; set; } = "";
        public string Feature2 { get; set; } = "";
        public string Feature3 { get; set; } = "";
        public string Feature4 { get; set; } = "";

        // See the matching comment on CreateMaterialConsumptionEntryRequestAPIModel - mirrors
        // od_tpdt1.prg's "Calculate Consumptions...? Yes|No" dialog.
        public bool CalculateConsumption { get; set; } = true;

        // Not [Required] - legitimately blank when CalculateConsumption is false.
        public string ConsumptionUnit { get; set; } = "";
        public decimal QuantityPerGarment { get; set; }
        public decimal PercentageAllowance { get; set; }
        [Required] public string ItemUnit { get; set; } = null!;
        [Required] public decimal TotalConsumption { get; set; }
        [Required] public string SupplierCode { get; set; } = null!;
        [Required] public decimal UnitPrice { get; set; }
        [Required] public string Currency { get; set; }
    }
}
