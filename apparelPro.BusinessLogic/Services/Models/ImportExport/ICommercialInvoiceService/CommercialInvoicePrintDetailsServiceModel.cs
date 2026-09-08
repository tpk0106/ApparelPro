namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICommercialInvoiceService
{
    // Everything the printed Commercial Invoice needs beyond the header's own
    // fields - names/addresses resolved from Buyer+Address, and the Shipper
    // block from CompanyAddressSetup (legacy ie_setup->p_head1/p_add1-3).
    public class CommercialInvoicePrintDetailsServiceModel
    {
        public CommercialInvoiceHeaderServiceModel Header { get; set; } = null!;
        public List<CommercialInvoiceLineServiceModel> Lines { get; set; } = new();

        public string ShipperCompanyName { get; set; } = "";
        public string ShipperAddress1 { get; set; } = "";
        public string ShipperAddress2 { get; set; } = "";
        public string ShipperAddress3 { get; set; } = "";

        public string BuyerName { get; set; } = "";

        public string ConsigneeName { get; set; } = "";
        public List<string> ConsigneeAddressLines { get; set; } = new();

        public string NotifyPartyName { get; set; } = "";
        public List<string> NotifyPartyAddressLines { get; set; } = new();

        public string LoadPortDescription { get; set; } = "";
        public string DestinationDescription { get; set; } = "";
        public string IssuingBankName { get; set; } = "";
    }
}
