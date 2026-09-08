namespace apparelPro.BusinessLogic.Services.Models.ImportExport.ICommercialInvoiceService
{
    public class CommercialInvoiceDetailServiceModel
    {
        public CommercialInvoiceHeaderServiceModel Header { get; set; } = null!;
        public List<CommercialInvoiceLineServiceModel> Lines { get; set; } = new();
    }
}
