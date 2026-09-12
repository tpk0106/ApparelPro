namespace ApparelPro.Data.Models.ImportExport
{
    // Matches legacy ie_pack2.dbf - the "Carton" packaging-media breakdown.
    // One row per (FromCartonNo, ToCartonNo, Color, Size) combination for a
    // given Commercial Invoice line - legacy's own dynamic per-size columns
    // (dbedit array built at runtime from the style's real sizes) collapse
    // to one flat row per size here, same as the source DBF.
    public class PackingListCartonDetail
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public int BuyerCode { get; set; }
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public string NewOrder { get; set; } = "";
        public int FromCartonNo { get; set; }
        public int ToCartonNo { get; set; }
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public decimal Qty { get; set; }
        public int NoOfCartons { get; set; }
    }
}
