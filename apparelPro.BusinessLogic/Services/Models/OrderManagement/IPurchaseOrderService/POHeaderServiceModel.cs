using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class POHeaderServiceModel
    {
        // True only when the user is raising a brand-new P/O (New P/O Entry mode).
        // When true, the backend allocates the real P/O number itself via the
        // shared document sequence service and ignores PurchaseNumber below.
        // When false (Edit Existing P/O), PurchaseNumber must reference an
        // already-existing P/O.
        public bool IsNewPurchaseOrder { get; set; }

        // Not [Required]: for a new P/O this arrives blank (the backend
        // assigns it). SaveSupplierPurchaseOrderAsync enforces it's present
        // when IsNewPurchaseOrder is false instead.
        public string PurchaseNumber { get; set; } = "";
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
