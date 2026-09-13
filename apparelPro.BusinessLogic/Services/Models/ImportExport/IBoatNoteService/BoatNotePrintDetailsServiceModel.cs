namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IBoatNoteService
{
    // Everything the printed Boat Note needs beyond the header's own fields -
    // Shipper resolved from CompanyAddress, Consignee reused from Commercial
    // Invoice's own resolution (same as Certificate of Origin), and the
    // Port of Loading/Discharge descriptions resolved from Destinations.
    public class BoatNotePrintDetailsServiceModel
    {
        public BoatNoteHeaderServiceModel Header { get; set; } = null!;
        public List<BoatNoteCargoLineServiceModel> Lines { get; set; } = new();
        public string ShipperName { get; set; } = "";
        public string ConsigneeName { get; set; } = "";
        public List<string> ConsigneeAddressLines { get; set; } = new();
        public string PortOfLoadingDescription { get; set; } = "";
        public string DischargePortDescription { get; set; } = "";
    }
}
