namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    // Edits an existing GeneralStockMaster row by its already-known composite
    // ItemCode - see IGeneralStockMasterService.UpdateGeneralStockMasterAsync for why
    // this is deliberately separate from GeneralStockMasterEntryServiceModel (which
    // creates new rows via the StockItems/OrderItemFeatures catalog).
    public class GeneralStockMasterUpdateServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // full 22-char composite

        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
    }
}
