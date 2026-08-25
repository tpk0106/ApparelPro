namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralPoHeaderServiceModel
    {
        // Empty/blank on a new P/O - server allocates one (NoteType "GPO"). A non-empty
        // value looks up and updates an existing P/O, mirroring legacy's "New Purchase
        // Order...? Yes/No" prompt.
        public string PoNumber { get; set; } = "";
        public bool IsNewPurchaseOrder { get; set; }

        public string SupplierCode { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public string BasisCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public string? ProformaInvoiceNo { get; set; }
        public DateTime? ProformaInvoiceDate { get; set; }
    }
}
