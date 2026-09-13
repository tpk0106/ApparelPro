namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class BoatNoteHeaderAPIModel
    {
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
        public string? CustomsOfficerStatus { get; set; }
        public string? CustomsOfficerReference { get; set; }
        public string? TerminalOperatorReleaseStatus { get; set; }
        public string? TerminalOperatorReference { get; set; }
        public string? ShipperAgentSignOffStatus { get; set; }
        public string? ChaLicenseNo { get; set; }
    }
}
