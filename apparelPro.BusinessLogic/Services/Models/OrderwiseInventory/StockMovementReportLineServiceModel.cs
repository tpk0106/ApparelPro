namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockMovementReportLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal RequisitionedQuantity { get; set; }
        public decimal IssuedQuantity { get; set; }

        // Placeholder movement columns: no note-type module writes to these yet
        // (no Stock Transfer / Supplier Return / Adjustment Note feature exists in
        // OrderwiseStockTransactions today). They always compute to 0 for now — see
        // StockMovementReportService.TransactionSumByItem for the exact mechanism.
        // Wiring these up for real is purely a data-availability change, not a
        // report-code change, once those note types exist.
        public decimal TransferInQuantity { get; set; }
        public decimal TransferOutQuantity { get; set; }
        public decimal SupplierReturnQuantity { get; set; }
        public decimal LastAdjustmentQuantity { get; set; }

        // Real data today, aggregated across stores from OrderwiseStock.DamagedQuantity.
        public decimal DamagedQuantity { get; set; }

        // Derived, never persisted — same convention already used for
        // OrderwiseStockMaster's Issued/Received running totals.
        public decimal BalanceQuantity { get; set; }
    }
}
