namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    // "Regular" = returning good stock to the supplier (decrements QtyInHand/Value, same
    // as a GIN). "Damaged" = returning stock already written off via a DGN (decrements
    // only DamagedQuantity - the physical stock already left when it was marked damaged).
    public static class GeneralSrtnStockType
    {
        public const string Regular = "Regular";
        public const string Damaged = "Damaged";
    }

    public class GeneralSrtnHeaderServiceModel
    {
        public string SrtnNumber { get; set; } = null!; // allocated by the C# backend (NoteType "GSRTN")
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
        public int SupplierCode { get; set; }
        public string StockType { get; set; } = null!; // GeneralSrtnStockType.Regular / Damaged
    }
}
