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

        // RTN traceability addition: real data today (not a placeholder), read directly
        // off OrderwiseStockMaster.ReturnedQuantity - see that entity's comments. Feeds
        // into BalanceQuantity below per the "OrderedQuantity - IssuedQuantity +
        // ReturnedQuantity" convention documented on the entity itself.
        public decimal ReturnedQuantity { get; set; }

        // GTN traceability addition: real data today (not a placeholder), read directly
        // off OrderwiseStockMaster.TransferInQuantity/TransferOutQuantity - see that
        // entity's comments. Feed into BalanceQuantity below the same way
        // ReturnedQuantity does. (Confirmed against legacy IN_GTN1.PRG: transaction
        // codes "1T"/"6T", not the "TI"/"TO" placeholders originally proposed here
        // before that source file had been read.)
        public decimal TransferInQuantity { get; set; }
        public decimal TransferOutQuantity { get; set; }

        // SRN traceability addition: real data today (not a placeholder), read directly
        // off OrderwiseStockMaster.SupplierReturnQuantity - see that entity's comments.
        // Feeds into BalanceQuantity below the same way ReturnedQuantity/TransferIn/
        // TransferOut do. (Confirmed against legacy IN_SRN1.PRG: transaction code "7S",
        // not the "SR" placeholder originally proposed here before that source file had
        // been read.)
        public decimal SupplierReturnQuantity { get; set; }

        // Still placeholder: no Stock Adjustment Note module writes to
        // OrderwiseStockTransactions yet. Always computes to 0 for now — see
        // StockMovementReportService.TransactionSumByItem for the exact mechanism.
        public decimal LastAdjustmentQuantity { get; set; }

        // Real data today, aggregated across stores from OrderwiseStock.DamagedQuantity.
        public decimal DamagedQuantity { get; set; }

        // Derived, never persisted — same convention already used for
        // OrderwiseStockMaster's Issued/Received running totals.
        public decimal BalanceQuantity { get; set; }
    }
}
