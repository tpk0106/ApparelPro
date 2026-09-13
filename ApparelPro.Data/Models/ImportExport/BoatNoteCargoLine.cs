namespace ApparelPro.Data.Models.ImportExport
{
    // One row per container in the Boat Note's Cargo Summary - replaces
    // legacy's free-text marks/packages memo with real structured fields,
    // matching the real e-CDN sample the user supplied.
    public class BoatNoteCargoLine
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public int LineNo { get; set; }
        public string ContainerNo { get; set; } = "";
        public string? SealNo { get; set; }
        public string PackageQuantity { get; set; } = ""; // e.g. "450 Cartons"
        public string Description { get; set; } = "";
        public string? HsCode { get; set; }
        public decimal GrossWeight { get; set; }
        public string WeightUnit { get; set; } = "KG";
    }
}
