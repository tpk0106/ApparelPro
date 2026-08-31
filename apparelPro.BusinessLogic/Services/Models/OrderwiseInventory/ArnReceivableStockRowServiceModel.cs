namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ArnReceivableStockRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = "";
        public string AdditionalProcessCode { get; set; } = null!;
        public decimal ToDateIssued { get; set; }
        public decimal ToDateReceived { get; set; }

        // Legacy only caps entry for semi-finished garments ("valid m_qty <= m_bal_rec"
        // where m_bal_rec = to_dt_iss - to_dt_rec) - non-semi-finished items only require
        // Quantity > 0, no cap.
        public bool IsSemiFinishedGarment { get; set; }
        public decimal ReceivableBalance { get; set; }
    }
}
