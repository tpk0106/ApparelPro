namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockMovementReportLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal RequisitionedQuantity { get; set; }
        public decimal IssuedQuantity { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public decimal TransferInQuantity { get; set; }
        public decimal TransferOutQuantity { get; set; }
        public decimal SupplierReturnQuantity { get; set; }
        public decimal LastAdjustmentQuantity { get; set; }
        public decimal DamagedQuantity { get; set; }
        public decimal BalanceQuantity { get; set; }
    }
}
