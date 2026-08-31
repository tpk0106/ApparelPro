namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    // Modern equivalent of legacy GI_TPDT1.PRG's entry fields. StockCode/ItemCode come
    // from the same StockItems catalog Order Management's Material Consumption screen
    // already uses (api/material-consumption/catalog); Feature1-4 are free 4-char values
    // for whichever feature slots api/material-consumption/feature-headers reports as
    // defined for that StockCode+ItemCode (same composite-ItemCode convention as
    // StyleMaterialCostProfiles/GeneralStockReference elsewhere in this codebase).
    public class GeneralStockMasterEntryServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // base 4-char item code (not the composite)
        public string? Feature1 { get; set; }
        public string? Feature2 { get; set; }
        public string? Feature3 { get; set; }
        public string? Feature4 { get; set; }

        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
    }
}
