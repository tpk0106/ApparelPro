namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    public class StockItemMovementServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal ReceivedQuantity { get; set; }
        public decimal IssuedQuantity { get; set; }
        public decimal BalanceQuantity { get; set; }
        public decimal DamagedQuantity { get; set; }

        // true when issued/received >= 85% - the same "running low" signal
        // the dashboard alert list surfaces.
        public bool IsLow { get; set; }
    }
}
