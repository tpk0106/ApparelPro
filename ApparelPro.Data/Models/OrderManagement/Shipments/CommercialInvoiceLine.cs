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

        // Quota context - same sub-block shape as PartShipment's quota fields
        public string QuotaCategory { get; set; } = ""; // qta_cat
        public string FromYearMonth { get; set; } = ""; // f_yymm
        public string ToYearMonth { get; set; } = ""; // t_yymm
        public string QuotaCountry { get; set; } = ""; // qta_cont

        public string PackingMedia { get; set; } = ""; // pk_media
    }
}
