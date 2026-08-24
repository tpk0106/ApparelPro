namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.IShipmentStatusReportService
{
    // Replicates OD_SHPST.PRG's "SHIPMENT STATUS REPORT". Each row is one PartShipment
    // (Type/Style/shipment order) line; nested under it is every CommercialInvoiceLine
    // actually invoiced against that same Buyer+Order+Type+Style+NewOrder, mirroring the
    // legacy's own re-seek of ie_coin2 per printed od_part row.
    public class ShipmentStatusInvoiceLineServiceModel
    {
        public decimal QuantityShipped { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; } = null!;
    }

    public class ShipmentStatusRowServiceModel
    {
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;
        public string ShipmentOrderNo { get; set; } = null!; // new_order
        public string Unit { get; set; } = null!;
        public string DestinationCode { get; set; } = null!;
        public List<ShipmentStatusInvoiceLineServiceModel> InvoiceLines { get; set; } = new();
        // Legacy's running m_shipped total, scoped per printed row rather than across the
        // whole report (m_shipped is never reset to 0 between od_part rows in OD_SHPST.PRG,
        // but every row's invoice-line seek starts a fresh do-while, so the accumulation
        // visible in the original printout is effectively per-row anyway).
        public decimal TotalQuantityShipped { get; set; }
        public decimal BalanceToShip { get; set; } // PartShipment.Balance for this exact row
    }

    public class ShipmentStatusReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public List<ShipmentStatusRowServiceModel> Rows { get; set; } = new();
    }
}
