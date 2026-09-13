namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IValueDeclarationService
{
    // Everything the printed Form 308A needs beyond the header/line's own
    // codes - resolved descriptions for every reference-table code, so the
    // print engine never has to look anything up itself.
    public class ValueDeclarationPrintDetailsServiceModel
    {
        public ValueDeclarationHeaderServiceModel Header { get; set; } = null!;
        public List<ValueDeclarationLinePrintServiceModel> Lines { get; set; } = new();
        public string ImporterCompanyName { get; set; } = "";
        public string ImporterAddress1 { get; set; } = "";
        public string ImporterAddress2 { get; set; } = "";
        public string ImporterCityPostCodeCountry { get; set; } = "";

        // Invoice-scoped flow only - Exporter is our own company (resolved
        // from CompanyAddressId), so unlike the standalone flow's plain
        // Header.ExporterName/ExporterAddress strings, these are resolved
        // the same way Importer is above.
        public string? ExporterCompanyName { get; set; }
        public string? ExporterAddress1 { get; set; }
        public string? ExporterAddress2 { get; set; }
        public string? ExporterCityPostCodeCountry { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public string CurrencyDescription { get; set; } = "";
        public string TermsOfDeliveryDescription { get; set; } = "";
        public string TermsOfPaymentDescription { get; set; } = "";
        public string PortOfShipmentDescription { get; set; } = "";
    }

    public class ValueDeclarationLinePrintServiceModel : ValueDeclarationLineServiceModel
    {
        public string CountryOfOriginDescription { get; set; } = "";
        public string UnitDescription { get; set; } = "";
    }
}
