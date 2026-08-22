namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class StockItemMovementAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal ReceivedQuantity { get; set; }
        public decimal IssuedQuantity { get; set; }
        public decimal BalanceQuantity { get; set; }
        public decimal DamagedQuantity { get; set; }
        public bool IsLow { get; set; }
    }
}
