namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockMovementReportLineServiceModel
    {
        public DateOnly TransactionDate { get; set; }
        public TimeOnly? TransactionTime { get; set; }
        public string TransactionTypeCode { get; set; } = null!;
        public string DocumentTypeDescription { get; set; } = "";
        public string DocumentNumber { get; set; } = null!;

        // "In", "Out", or "-" (Stock Adjustment / Damaged Supplier Return / Stores
        // Requisition reservations - none of these move QtyInHand directionally).
        public string Status { get; set; } = "";

        // Free-form counterpart detail - Supplier+Invoice, Buyer+Order, Department,
        // or the other store's name for a General GTN leg. Blank where legacy shows none.
        public string SourceTarget { get; set; } = "";

        public decimal Amount { get; set; }
        public decimal RunningBalance { get; set; }
    }
}
