namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class CommercialInvoiceDetailAPIModel
    {
        public CommercialInvoiceHeaderAPIModel Header { get; set; } = null!;
        public List<CommercialInvoiceLineAPIModel> Lines { get; set; } = new();
    }
}
