namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_pack3.dbf - the "Container"/Hanging-Garments
    // packaging-media breakdown. One row per (BarNo, FromStringNo,
    // ToStringNo, Color, Size) combination for a given Commercial Invoice
    // line - same flat-per-size-row shape as PackingListCartonDetail.
    public class PackingListStringDetail
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public int BuyerCode { get; set; }
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public string NewOrder { get; set; } = "";
        public int BarNo { get; set; }
        public int FromStringNo { get; set; }
        public int ToStringNo { get; set; }
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public decimal Qty { get; set; }
    }
}
