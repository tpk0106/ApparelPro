using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class POHeaderAPIModel
    {
        [Required] public string PurchaseNumber { get; set; } = null!;
        [Required] public int SupplierCode { get; set; }
        [Required] public string StoreCode { get; set; } = null!;
        public string ProformaInvoiceNo { get; set; } = "";
        public DateTime? ProformaInvoiceDate { get; set; }
        [Required] public string CurrencyCode { get; set; } = null!;
        [Required] public int BuyerCode { get; set; }
        [Required] public string OrderNumber { get; set; } = null!;
    }
}
