namespace ApparelPro.Data.Models.ImportExport
{
    // Matches a real Sri Lanka Customs e-CDN "Boat Note" (goods passed out of
    // Customs control) - a modern digital release document, not legacy
    // ie_shp's DOS-era "Shipping Note/Boat Note" (IE_SHPN1-3.PRG), which this
    // migration deliberately does not reproduce. One Boat Note per Commercial
    // Invoice, same as Certificate of Origin.
    public class BoatNoteHeader
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public string? BoatNoteNumber { get; set; }
        public DateTime? BoatNoteDateTime { get; set; }
        public string? CustomsRegNo { get; set; }
        public string? CusDecRef { get; set; }
        public int CompanyAddressId { get; set; }
        public string? VesselName { get; set; }
        public string? VoyageNo { get; set; }
        public string? PortOfLoadingCode { get; set; }
        public string? DischargePortCode { get; set; }
        public string? Remarks { get; set; }

        // "Digital Verifications & Releases" - status/reference text only,
        // no real approval workflow (per explicit scope decision).
        public string? CustomsOfficerStatus { get; set; }
        public string? CustomsOfficerReference { get; set; }
        public string? TerminalOperatorReleaseStatus { get; set; }
        public string? TerminalOperatorReference { get; set; }
        public string? ShipperAgentSignOffStatus { get; set; }
        public string? ChaLicenseNo { get; set; }
    }
}
