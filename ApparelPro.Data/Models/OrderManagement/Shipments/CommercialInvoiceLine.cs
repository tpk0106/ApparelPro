namespace ApparelPro.Data.Models.OrderManagement.Shipments
{
    // Maps legacy ie_coin2 - one row per quantity actually invoiced/shipped
    // against a PartShipment's shipment order (new_order), linked to the
    // CommercialInvoiceHeader that carries its ship date.
    public class CommercialInvoiceLine
    {
        public int Id { get; set; } // Internal database primary auto-increment identity seed

        public string InvoiceNumber { get; set; } = null!; // invo_no

        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string NewOrder { get; set; } = null!; // new_order (Split Shipping Order Ref)

        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; } // qty - quantity invoiced on this line
        public decimal Balance { get; set; } // balance - remaining balance as of this invoice

        public string PackingMedia { get; set; } = ""; // pk_media
    }
}
