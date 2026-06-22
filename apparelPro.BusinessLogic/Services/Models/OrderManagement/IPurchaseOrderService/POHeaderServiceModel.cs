using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class POHeaderServiceModel
    {
        [Required] public string PurchaseNumber { get; set; } = null!;
        [Required] public int SupplierCode { get; set; }
        [Required] public int TypeCode { get; set; }
        [Required] public string StyleCode { get; set; }
        [Required] public string StoreCode { get; set; } = null!;
        public string ProformaInvoiceNo { get; set; } = "";
        public DateOnly? ProformaInvoiceDate { get; set; }
        [Required] public string CurrencyCode { get; set; } = null!;
        [Required] public int BuyerCode { get; set; }
        [Required] public string OrderNumber { get; set; } = null!;
    }
}
