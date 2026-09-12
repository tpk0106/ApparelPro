namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_pack1.dbf - one row per Commercial Invoice line
    // (same composite business key as CommercialInvoiceLine), holding just
    // the free-text packing detail memo (Marks & Nos / Description of
    // Goods / Quantity / Unit / Weight / Measurement narrative).
    public class PackingListLine
    {
        public string InvoiceNumber { get; set; } = "";
        public int BuyerCode { get; set; }
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public string NewOrder { get; set; } = "";
        public string? Detail { get; set; }
    }
}
