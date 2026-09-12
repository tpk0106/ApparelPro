namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsDeclarationService
{
    // Everything the printed CUSDEC I/II needs beyond the header/lines' own
    // codes - names/descriptions resolved from Buyer and the various small
    // reference-code masters, so the printed form shows readable text
    // instead of raw codes wherever the real form has room for it.
    public class CustomsDeclarationPrintDetailsServiceModel
    {
        public CustomsDeclarationHeaderServiceModel Header { get; set; } = null!;
        public List<CustomsDeclarationLineServiceModel> Lines { get; set; } = new();
        public List<CustomsDeclarationAttachedDocumentServiceModel> AttachedDocuments { get; set; } = new();

        public string ExporterName { get; set; } = "";
        public string ConsigneeName { get; set; } = "";
        public string NotifyPartyName { get; set; } = "";
        public string DeclarantBuyerName { get; set; } = "";
        public string ClearanceOfficeDescription { get; set; } = "";
        public string FrontierOfficeDescription { get; set; } = "";
        public string PaymentTermDescription { get; set; } = "";
        public string DeliveryTermDescription { get; set; } = "";
        public string TransportModeDescription { get; set; } = "";
        public string CountryOfConsignmentName { get; set; } = "";
        public string CountryOfOriginName { get; set; } = "";
        public string CountryOfDestinationName { get; set; } = "";
        public string PortOfLoadingName { get; set; } = "";
        public string PortOfDischargeName { get; set; } = "";
        public string PlaceOfDeliveryName { get; set; } = "";
        public string BankName { get; set; } = "";

        // Keyed by the raw code so the print engine can look up per-line
        // descriptions without another round trip.
        public Dictionary<string, string> CommodityDescriptions { get; set; } = new();
        public Dictionary<string, string> CustomsProcedureDescriptions { get; set; } = new();
        public Dictionary<string, string> AgreementDescriptions { get; set; } = new();
        public Dictionary<string, string> CountryNames { get; set; } = new();
        public Dictionary<string, string> UnitDescriptions { get; set; } = new();
        public Dictionary<string, string> TaxDescriptions { get; set; } = new();
        public Dictionary<string, string> TaxBaseDescriptions { get; set; } = new();
        public Dictionary<string, string> DocumentTypeDescriptions { get; set; } = new();
    }
}
