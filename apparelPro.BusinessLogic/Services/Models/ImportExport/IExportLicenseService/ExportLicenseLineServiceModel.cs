namespace apparelPro.BusinessLogic.Services.Models.ImportExport.IExportLicenseService
{
    public class ExportLicenseLineServiceModel
    {
        public int Id { get; set; }
        public int ExportLicenseHeaderId { get; set; }
        public int ItemNo { get; set; }
        public string? HsNumber { get; set; }
        public string Description { get; set; } = "";
        public string? PackSize { get; set; }
        public string? UnitCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Insurance { get; set; }
        public decimal Freight { get; set; }
        public decimal TotalCif { get; set; }
    }
}
